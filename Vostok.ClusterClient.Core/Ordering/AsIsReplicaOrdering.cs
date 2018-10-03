using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;

namespace Vostok.ClusterClient.Core.Ordering
{
    /// <summary>
    /// Represents an ordering which never changes replicas order.
    /// </summary>
    [PublicAPI]
    public class AsIsReplicaOrdering : IReplicaOrdering
    {
        /// <inheritdoc />
        public IEnumerable<Uri> Order(IList<Uri> replicas, IReplicaStorageProvider storageProvider, Request request) =>
            replicas;

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}