using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies.DelayProviders;

namespace Vostok.Clusterclient.Core.Strategies
{
    /// <summary>
    /// <para>Represents a strategy which starts with one request, but can increase parallelism ("fork") when there's no response for long enough.</para>
    /// <para>Forking occurs when the strategy does not receive any responses during a time period called forking delay. Forking delays are provided by <see cref="IForkingDelaysProvider"/> implementations.</para>
    /// <para>Parallelism level can only increase during execution due to forking, but never decreases. However, it has a configurable upper bound.</para>
    /// <para>Execution stops at any result with <see cref="ResponseVerdict.Accept"/> verdict.</para>
    /// </summary>
    /// <example>
    /// Example of execution with maximum parallelism = 3:
    /// <code>
    /// o---------------------------------- (replica1) ----------------------->
    ///           | (fork)
    ///           o-----------------------X (replica2) o-----------> (replica4)
    ///                     | (fork)
    ///                     o-------------- (replica3) ------> V (success!)
    /// </code>
    /// <code>
    /// ----------------------------------------------------------------------------> (time)
    /// | delay1  |  delay2 |             ↑ failure(replica2)  ↑ success(replica3)
    /// </code>
    /// </example>
    [PublicAPI]
    public class ForkingRequestStrategy : IRequestStrategy
    {
        private readonly IForkingDelaysProvider delaysProvider;
        private readonly IForkingDelaysPlanner delaysPlanner;
        private readonly int maximumParallelism;

        public ForkingRequestStrategy([NotNull] IForkingDelaysProvider delaysProvider, int maximumParallelism)
            : this(delaysProvider, ForkingDelaysPlanner.Instance, maximumParallelism)
        {
        }

        internal ForkingRequestStrategy([NotNull] IForkingDelaysProvider delaysProvider, [NotNull] IForkingDelaysPlanner delaysPlanner, int maximumParallelism)
        {
            if (delaysProvider == null)
                throw new ArgumentNullException(nameof(delaysProvider));

            if (delaysPlanner == null)
                throw new ArgumentNullException(nameof(delaysPlanner));

            if (maximumParallelism <= 0)
                throw new ArgumentOutOfRangeException(nameof(maximumParallelism), "Maximum parallelism level must be a positive number.");

            this.delaysProvider = delaysProvider;
            this.delaysPlanner = delaysPlanner;
            this.maximumParallelism = maximumParallelism;
        }

        /// <inheritdoc />
        public async Task SendAsync(Request request, RequestParameters parameters, IRequestSender sender, IRequestTimeBudget budget, IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken)
        {
            var currentTasks = new List<Task>(Math.Min(maximumParallelism, replicasCount));

            using (var localCancellationSource = new CancellationTokenSource())
            using (var linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, localCancellationSource.Token))
            {
                var linkedCancellationToken = linkedCancellationSource.Token;

                using (var replicasEnumerator = replicas.GetEnumerator())
                {
                    for (var i = 0; i < replicasCount; i++)
                    {
                        if (budget.HasExpired)
                            break;

                        if (request.ContainsAlreadyUsedStream())
                            break;

                        var connectionAttemptTimeout = i == replicasCount - 1 ? null : parameters.ConnectionTimeout;

                        LaunchRequest(currentTasks, request, budget, sender, replicasEnumerator, connectionAttemptTimeout, linkedCancellationToken);

                        ScheduleForkIfNeeded(currentTasks, request, budget, i, replicasCount, linkedCancellationToken);

                        if (await WaitForAcceptedResultAsync(currentTasks).ConfigureAwait(false))
                        {
                            localCancellationSource.Cancel();
                            return;
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                while (currentTasks.Count > 0)
                {
                    if (budget.HasExpired || await WaitForAcceptedResultAsync(currentTasks).ConfigureAwait(false))
                        return;
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"Forking({delaysProvider})";

        private static async Task<bool> WaitForAcceptedResultAsync(List<Task> currentTasks)
        {
            var completedTask = await Task.WhenAny(currentTasks).ConfigureAwait(false);

            currentTasks.Remove(completedTask);

            var resultTask = completedTask as Task<ReplicaResult>;
            if (resultTask == null)
                return false;

            currentTasks.RemoveAll(task => !(task is Task<ReplicaResult>));

            var result = await resultTask.ConfigureAwait(false);

            return result.Verdict == ResponseVerdict.Accept;
        }

        private void LaunchRequest(List<Task> currentTasks, Request request, IRequestTimeBudget budget, IRequestSender sender, IEnumerator<Uri> replicasEnumerator, TimeSpan? connectionTimeout, CancellationToken cancellationToken)
        {
            if (!replicasEnumerator.MoveNext())
                throw new InvalidOperationException("Replicas enumerator ended prematurely. This is definitely a bug in code.");

            request = request.WithHeader(HeaderNames.ConcurrencyLevel, currentTasks.Count(task => task is Task<ReplicaResult>) + 1);

            currentTasks.Add(sender.SendToReplicaAsync(replicasEnumerator.Current, request, connectionTimeout, budget.Remaining, cancellationToken));
        }

        private void ScheduleForkIfNeeded(List<Task> currentTasks, Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas, CancellationToken cancellationToken)
        {
            if (currentReplicaIndex == totalReplicas - 1)
                return;

            if (currentTasks.Count >= maximumParallelism)
                return;

            var forkingDelay = delaysProvider.GetForkingDelay(request, budget, currentReplicaIndex, totalReplicas);
            if (forkingDelay == null)
                return;

            if (forkingDelay.Value < TimeSpan.Zero)
                return;

            if (forkingDelay.Value >= budget.Remaining)
                return;

            currentTasks.Add(delaysPlanner.Plan(forkingDelay.Value, cancellationToken));
        }
    }
}