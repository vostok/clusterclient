using System;
using System.Collections.Concurrent;

namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    internal class PerInstanceReplicaStorageProvider : IReplicaStorageProvider
    {
        private readonly ConcurrentDictionary<Type, object> containers = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, object> globalContainers = new ConcurrentDictionary<Type, object>();
        
        public ConcurrentDictionary<Uri, TValue> Obtain<TValue>(string storageKey = null) =>
            ObtainContainer<TValue>().Obtain(storageKey);

        public TValue ObtainGlobalValue<TValue>(string storageKey, Func<TValue> factory) =>
            ObtainGlobalContainer<TValue>().Obtain(storageKey, factory);

        private ReplicaStorageContainer<TValue> ObtainContainer<TValue>() =>
            (ReplicaStorageContainer<TValue>) containers.GetOrAdd(typeof(TValue), _ => new ReplicaStorageContainer<TValue>());

        private GlobalStorageContainer<TValue> ObtainGlobalContainer<TValue>()
            => (GlobalStorageContainer<TValue>)globalContainers.GetOrAdd(typeof(TValue), _ => new GlobalStorageContainer<TValue>());
    }
}