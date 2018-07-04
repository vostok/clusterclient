using System;
using System.Collections.Generic;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;

namespace Vostok.ClusterClient.Core.Ordering
{
    /// <summary>
    /// Represents an ordering which never changes replicas order.
    /// </summary>
    public class AsIsReplicaOrdering : IReplicaOrdering
    {
        public IEnumerable<Uri> Order(IList<Uri> replicas, IReplicaStorageProvider storageProvider, Request request) =>
            replicas;

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}