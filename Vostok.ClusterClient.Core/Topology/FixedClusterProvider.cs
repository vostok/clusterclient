using System;
using System.Collections.Generic;
using Vostok.ClusterClient.Abstractions.Topology;

namespace Vostok.ClusterClient.Core.Topology
{
    /// <summary>
    /// Represents a cluster provider which always returns a fixed list of urls.
    /// </summary>
    public class FixedClusterProvider : IClusterProvider
    {
        private readonly IList<Uri> replicas;

        public FixedClusterProvider(IList<Uri> replicas)
        {
            this.replicas = replicas;
        }

        public FixedClusterProvider(params Uri[] replicas)
        {
            this.replicas = replicas;
        }

        public IList<Uri> GetCluster() => replicas;
    }
}