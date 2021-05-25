using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    [PublicAPI]
    public class PerProcessGlobalStorageProvider : IGlobalStorageProvider
    {
        public TValue ObtainGlobalValue<TValue>(string storageKey, Func<TValue> factory) =>
            GlobalStorageContainer<TValue>.Shared.Obtain(storageKey, factory);
    }
}