using System;
using System.Collections.Generic;

namespace Vostok.ClusterClient.Core.Topology
{
    /// <summary>
    /// Represents a cluster provider which uses an external delegate to provide replica urls.
    /// </summary>
    public class AdHocClusterProvider : IClusterProvider
    {
        private readonly Func<IList<Uri>> replicasProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="AdHocClusterProvider"/> class.
        /// </summary>
        /// <param name="replicasProvider">An external delegate which will provides replica urls.</param>
        public AdHocClusterProvider(Func<IList<Uri>> replicasProvider)
        {
            this.replicasProvider = replicasProvider;
        }

        /// <inheritdoc />
        public IList<Uri> GetCluster() => replicasProvider();
    }
}