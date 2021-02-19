using System;
using System.Collections.Concurrent;

namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    internal class GlobalStorageContainer<TValue>
    {
        public static readonly GlobalStorageContainer<TValue> Shared = new GlobalStorageContainer<TValue>();

        private readonly ConcurrentDictionary<string, TValue> values = new ConcurrentDictionary<string, TValue>();

        public TValue Obtain(string storageKey, Func<TValue> factory) =>
            values.TryGetValue(storageKey, out var value) ? value : values.GetOrAdd(storageKey, _ => factory());
    }
}