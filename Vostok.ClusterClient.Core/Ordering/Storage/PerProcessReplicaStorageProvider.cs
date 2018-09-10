using System;
using System.Collections.Concurrent;
using Vostok.ClusterClient.Abstractions.Ordering.Storage;

namespace Vostok.ClusterClient.Core.Ordering.Storage
{
    internal class PerProcessReplicaStorageProvider : IReplicaStorageProvider
    {
        public ConcurrentDictionary<Uri, TValue> Obtain<TValue>(string storageKey = null) =>
            ReplicaStorageContainer<TValue>.Shared.Obtain(storageKey);
    }
}