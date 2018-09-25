using System;
using System.Collections.Generic;

namespace Vostok.ClusterClient.Core.Topology
{
    /// <summary>
    /// Represents a cluster provider which always returns a fixed list of urls.
    /// </summary>
    public class FixedClusterProvider : IClusterProvider
    {
        private readonly IList<Uri> replicas;

        /// <summary>
        /// Initializes a new instance of <see cref="FixedClusterProvider"/> class.
        /// </summary>
        /// <param name="replicas">A list of replica <see cref="Uri"/> which this provider should return.</param>
        public FixedClusterProvider(IList<Uri> replicas)
        {
            this.replicas = replicas;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FixedClusterProvider"/> class.
        /// </summary>
        /// <param name="replicas">A list of replica <see cref="Uri"/> which this provider should return.</param>
        public FixedClusterProvider(params Uri[] replicas)
        {
            this.replicas = replicas;
        }

        /// <inheritdoc />
        public IList<Uri> GetCluster() => replicas;
    }
}