using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Ordering.Storage;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;

namespace Vostok.ClusterClient.Core.Ordering.Weighed
{
    internal interface IReplicaWeightCalculator
    {
        double GetWeight(
            [NotNull] Uri replica,
            [NotNull] IList<Uri> allReplicas,
            [NotNull] IReplicaStorageProvider storageProvider,
            [NotNull] Request request);
    }
}