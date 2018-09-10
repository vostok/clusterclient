using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Represents a strategy which only sends a request to a single, first replica, using all available time budget.
    /// </summary>
    /// <example>
    /// <code>
    /// o--------------------- (replica) -----------> X (failure)
    /// o--------------------- (replica) -----------> V (success)
    /// </code>
    /// </example>
    public class SingleReplicaRequestStrategy : IRequestStrategy
    {
        public Task SendAsync(Request request, IRequestSender sender, IRequestTimeBudget budget, IEnumerable<Uri> replicas, int replicasCount, CancellationToken cancellationToken) =>
            sender.SendToReplicaAsync(replicas.First(), request, budget.Remaining, cancellationToken);

        public override string ToString() => "SingleReplica";
    }
}