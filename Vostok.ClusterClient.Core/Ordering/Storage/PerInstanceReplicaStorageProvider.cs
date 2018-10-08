using System;
using System.Collections.Concurrent;

namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    internal class PerInstanceReplicaStorageProvider : IReplicaStorageProvider
    {
        private readonly ConcurrentDictionary<Type, object> containers;

        public PerInstanceReplicaStorageProvider() =>
            containers = new ConcurrentDictionary<Type, object>();

        public ConcurrentDictionary<Uri, TValue> Obtain<TValue>(string storageKey = null) =>
            ObtainContainer<TValue>().Obtain(storageKey);

        private ReplicaStorageContainer<TValue> ObtainContainer<TValue>() =>
            (ReplicaStorageContainer<TValue>) containers.GetOrAdd(typeof(TValue), _ => new ReplicaStorageContainer<TValue>());
    }
}