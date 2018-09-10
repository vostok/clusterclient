using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Sending;
using Vostok.ClusterClient.Abstractions.Strategies;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Sending;

namespace Vostok.ClusterClient.Core.Strategies
{
    /// <summary>
    /// Represents a strategy which maintains several parallel requests right from the start and stops at any result with <see cref="ResponseVerdict.Accept"/> verdict.
    /// </summary>
    /// <example>
    /// Example of execution with parallellism = 3:
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
    public class ParallelRequestStrategy : IRequestStrategy
    {
        public ParallelRequestStrategy(int parallelismLevel)
        {
            if (parallelismLevel <= 0)
                throw new ArgumentOutOfRangeException(nameof(parallelismLevel), "Parallelism level must be a positive number.");

            ParallelismLevel = parallelismLevel;
        }

        public int ParallelismLevel { get; }

        public async Task SendAsync(Request request, IRequestSender sender, IRequestTimeBudget budget, IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken)
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

                        currentTasks.Add(sender.SendToReplicaAsync(replicasEnumerator.Current, request, budget.Remaining, linkedCancellationToken));
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

                        TryLaunchNextRequest(request, sender, budget, replicasEnumerator, currentTasks, linkedCancellationToken);
                    }
                }
            }
        }

        public override string ToString() => "Parallel-" + ParallelismLevel;

        private static void TryLaunchNextRequest(Request request, IRequestSender sender, IRequestTimeBudget budget, IEnumerator<Uri> replicas, List<Task<ReplicaResult>> currentTasks, CancellationToken cancellationToken)
        {
            if (budget.HasExpired)
                return;

            if (request.ContainsAlreadyUsedStream())
                return;

            if (replicas.MoveNext())
                currentTasks.Add(sender.SendToReplicaAsync(replicas.Current, request, budget.Remaining, cancellationToken));
        }
    }
}