using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies.TimeoutProviders;
using Vostok.Commons.Time;

namespace Vostok.Clusterclient.Core.Strategies
{
    /// <summary>
    /// <para>Represents a strategy which traverses replicas sequentially, does not use parallelism and stops at any result with <see cref="ResponseVerdict.Accept"/> verdict.</para>
    /// <para>Request timeouts for each attempt are given by an implementation of <see cref="ISequentialTimeoutsProvider"/> interface.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// o--------X (replica1) o--------------X (replica2) o----------> V (replica3)
    /// </code>
    /// <code>
    /// ------------------------------------------------------------------------------------> (time)
    ///          ↑ failure(replica1)         ↑ failure(replica2)       ↑ success(replica3)
    /// </code>
    /// </example>
    [PublicAPI]
    public class SequentialRequestStrategy : IRequestStrategy
    {
        private readonly ISequentialTimeoutsProvider timeoutsProvider;

        /// <param name="timeoutsProvider">A timeout provider which will be used by strategy.</param>
        public SequentialRequestStrategy([NotNull] ISequentialTimeoutsProvider timeoutsProvider)
        {
            this.timeoutsProvider = timeoutsProvider ?? throw new ArgumentNullException(nameof(timeoutsProvider));
        }

        /// <inheritdoc />
        public async Task SendAsync(Request request, IRequestSender sender, IRequestTimeBudget budget, IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken)
        {
            var currentReplicaIndex = 0;

            foreach (var replica in replicas)
            {
                if (budget.HasExpired)
                    break;

                if (request.ContainsAlreadyUsedStream())
                    break;

                var timeout = TimeSpanArithmetics.Min(timeoutsProvider.GetTimeout(request, budget, currentReplicaIndex++, replicasCount), budget.Remaining);

                var result = await sender.SendToReplicaAsync(replica, request, timeout, cancellationToken).ConfigureAwait(false);
                if (result.Verdict == ResponseVerdict.Accept)
                    break;

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"Sequential({timeoutsProvider})";
    }
}