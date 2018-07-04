using System;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive;

namespace Vostok.ClusterClient.Core
{
    public static class ClusterClientCachesCleaner
    {
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