using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;

namespace Vostok.Clusterclient.Core.Strategies
{
    /// <summary>
    /// Represents a strategy which maintains several parallel requests right from the start and stops at any result with <see cref="ResponseVerdict.Accept"/> verdict.
    /// </summary>
    /// <example>
    /// Example of execution with parallelism = 3:
    /// <code>
    /// o-------------- (replica1) -------------------------------->
    /// o-------------- (replica2) ----X o------ (replica4) ------->
    /// o-------------- (replica3) ------------------------------- V (success)
    /// </code>
    /// <code>
    /// -------------------------------------------------------------------------------> (time)
    ///                                ↑ failure(replica2)         ↑ success(replica3)
    /// </code>
    /// </example>
    [PublicAPI]
    public class ParallelRequestStrategy : IRequestStrategy
    {
        /// <param name="parallelismLevel">A maximal parallelism level.</param>
        public ParallelRequestStrategy(int parallelismLevel)
        {
            if (parallelismLevel <= 0)
                throw new ArgumentOutOfRangeException(nameof(parallelismLevel), "Parallelism level must be a positive number.");

            ParallelismLevel = parallelismLevel;
        }

        /// <summary>
        /// A maximal parallelism level.
        /// </summary>
        public int ParallelismLevel { get; }

        /// <inheritdoc />
        public async Task SendAsync(Request request, RequestParameters parameters, IRequestSender sender, IRequestTimeBudget budget, IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken)
        {
            var initialRequestCount = Math.Min(ParallelismLevel, replicasCount);
            var currentTasks = new List<Task<ReplicaResult>>(initialRequestCount);

            using (var localCancellationSource = new CancellationTokenSource())
            using (var linkedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, localCancellationSource.Token))
            {
                var linkedCancellationToken = linkedCancellationSource.Token;

                using (var replicasEnumerator = replicas.GetEnumerator())
                {
                    for (var i = 0; i < initialRequestCount; i++)
                    {
                        if (!replicasEnumerator.MoveNext())
                            throw new InvalidOperationException("Replicas enumerator ended prematurely. This is definitely a bug in code.");

                        currentTasks.Add(sender.SendToReplicaAsync(replicasEnumerator.Current, request, null, budget.Remaining, linkedCancellationToken));
                    }

                    while (currentTasks.Count > 0)
                    {
                        var completedTask = await Task.WhenAny(currentTasks).ConfigureAwait(false);

                        currentTasks.Remove(completedTask);

                        var completedResult = await completedTask.ConfigureAwait(false);
                        if (completedResult.Verdict == ResponseVerdict.Accept)
                        {
                            localCancellationSource.Cancel();
                            return;
                        }

                        cancellationToken.ThrowIfCancellationRequested();

                        TryLaunchNextRequest(request, sender, budget, replicasEnumerator, currentTasks, parameters.ConnectionTimeout, linkedCancellationToken);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => "Parallel-" + ParallelismLevel;

        private static void TryLaunchNextRequest(Request request, IRequestSender sender, IRequestTimeBudget budget, IEnumerator<Uri> replicas, List<Task<ReplicaResult>> currentTasks, TimeSpan? connectionTimeout, CancellationToken cancellationToken)
        {
            if (budget.HasExpired)
                return;

            if (request.ContainsAlreadyUsedStream())
                return;

            if (replicas.MoveNext())
                currentTasks.Add(sender.SendToReplicaAsync(replicas.Current, request, connectionTimeout, budget.Remaining, cancellationToken));
        }
    }
}