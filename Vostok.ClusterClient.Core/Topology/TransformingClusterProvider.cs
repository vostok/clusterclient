using System;
using System.Collections.Generic;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.Commons.Collections;

namespace Vostok.ClusterClient.Core.Topology
{
    internal class TransformingClusterProvider : IClusterProvider
    {
        private readonly IReplicaTransform transform;

        private CachingTransform<IList<Uri>, IList<Uri>> cache;

        public TransformingClusterProvider(IClusterProvider provider, IReplicaTransform transform)
        {
            this.transform = transform;
            cache = new CachingTransform<IList<Uri>, IList<Uri>>(ApplyTransform, provider.GetCluster);
        }

        public IList<Uri> GetCluster()
            => cache.Get();

        private IList<Uri> ApplyTransform(IList<Uri> replicas)
            => replicas == null
                ? null
                : transform.Transform(replicas);
    }
}