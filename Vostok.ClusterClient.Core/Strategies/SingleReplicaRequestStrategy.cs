using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;

namespace Vostok.Clusterclient.Core.Strategies
{
    /// <summary>
    /// Represents a strategy which only sends a request to a single, first replica, using all available time budget.
    /// </summary>
    /// <example>
    /// <code>
    /// o--------------------- (replica) -----------> X (failure)
    /// o--------------------- (replica) -----------> V (success)
    /// </code>
    /// </example>
    [PublicAPI]
    public class SingleReplicaRequestStrategy : IRequestStrategy
    {
        /// <inheritdoc />
        public Task SendAsync(Request request, RequestParameters parameters, IRequestSender sender, IRequestTimeBudget budget, IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken) =>
            sender.SendToReplicaAsync(replicas.First(), request, null, budget.Remaining, cancellationToken);

        /// <inheritdoc />
        public override string ToString() => "SingleReplica";
    }
}