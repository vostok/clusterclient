using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Storage
{
    [PublicAPI]
    public interface IGlobalStorageProvider
    {
        /// <summary>
        /// <para>Return a global value created by <paramref name="factory"/>.</para>
        /// </summary>
        /// <typeparam name="TValue">Type of the storage values.</typeparam>
        /// <param name="storageKey">A unique string used to isolate objects with same value types.</param>
        /// <param name="factory">A factory by which object is created if not present in the storage.</param>
        /// <returns></returns>
        TValue ObtainGlobalValue<TValue>([NotNull] string storageKey, [NotNull] Func<TValue> factory);
    }
}