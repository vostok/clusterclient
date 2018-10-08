using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;

namespace Vostok.Clusterclient.Core.Ordering
{
    /// <summary>
    /// Represents an ordering which never changes replicas order.
    /// </summary>
    [PublicAPI]
    public class AsIsReplicaOrdering : IReplicaOrdering
    {
        /// <inheritdoc />
        public IEnumerable<Uri> Order(
            IList<Uri> replicas,
            IReplicaStorageProvider storageProvider,
            Request request,
            RequestParameters parameters) =>
            replicas;

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
        }
    }
}