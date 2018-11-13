using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed.Adaptive;

namespace Vostok.Clusterclient.Core
{
    /// <summary>
    /// A helper class for dropping ClusterClient caches.
    /// </summary>
    [PublicAPI]
    public static class ClusterClientCachesCleaner
    {
        /// <summary>
        /// Drops all ClusterClient caches. It also drops cache of <see cref="ReplicaBudgetingModule"/> and <see cref="AdaptiveThrottlingModule"/>.
        /// </summary>
        public static void Clean()
        {
            ReplicaBudgetingModule.ClearCache();

            AdaptiveThrottlingModule.ClearCache();

            ReplicaStorageContainer<int>.Shared.Clear();
            ReplicaStorageContainer<long>.Shared.Clear();
            ReplicaStorageContainer<bool>.Shared.Clear();
            ReplicaStorageContainer<double>.Shared.Clear();
            ReplicaStorageContainer<DateTime>.Shared.Clear();
            ReplicaStorageContainer<HealthWithDecay>.Shared.Clear();
        }
    }
}