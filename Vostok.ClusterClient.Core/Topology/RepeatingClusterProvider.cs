using System;
using System.Collections.Generic;
using Vostok.Commons.Collections;

namespace Vostok.ClusterClient.Core.Topology
{
    internal class RepeatingClusterProvider : IClusterProvider
    {
        private CachingTransform<IList<Uri>, IList<Uri>> cache;

        public RepeatingClusterProvider(IClusterProvider provider, int repeatCount)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (repeatCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(repeatCount), "Repeat count must be positive.");

            cache = new CachingTransform<IList<Uri>, IList<Uri>>(x => Repeat(x, repeatCount), provider.GetCluster);
        }

        public IList<Uri> GetCluster()
            => cache.Get();

        private static IList<Uri> Repeat(IList<Uri> currentReplicas, int repeatCount)
        {
            if (currentReplicas == null)
                return null;

            var repeatedReplicas = new List<Uri>(currentReplicas.Count * repeatCount);

            for (var i = 0; i < repeatCount; i++)
                repeatedReplicas.AddRange(currentReplicas);

            return repeatedReplicas;
        }
    }
}