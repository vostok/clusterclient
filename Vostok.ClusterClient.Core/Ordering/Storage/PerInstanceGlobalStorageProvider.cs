using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    [PublicAPI]
    public class PerInstanceGlobalStorageProvider : IGlobalStorageProvider
    {
        private readonly ConcurrentDictionary<Type, object> containers = new ConcurrentDictionary<Type, object>();

        public TValue ObtainGlobalValue<TValue>(string storageKey, Func<TValue> factory) =>
            ObtainContainer<TValue>().Obtain(storageKey, factory);

        private GlobalStorageContainer<TValue> ObtainContainer<TValue>() =>
            (GlobalStorageContainer<TValue>)containers.GetOrAdd(typeof(TValue), _ => new GlobalStorageContainer<TValue>());
    }
}