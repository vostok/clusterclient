using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;

namespace Vostok.Clusterclient.Core.Ordering.Weighed
{
    internal interface IReplicaWeightCalculator
    {
        double GetWeight(
            [NotNull] Uri replica,
            [NotNull] IList<Uri> allReplicas,
            [NotNull] IReplicaStorageProvider storageProvider,
            [NotNull] Request request,
            [NotNull] RequestParameters parameters);
    }
}