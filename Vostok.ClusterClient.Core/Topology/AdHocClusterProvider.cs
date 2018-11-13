using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Topology
{
    /// <summary>
    /// Represents a cluster provider which uses an external delegate to provide replica urls.
    /// </summary>
    [PublicAPI]
    public class AdHocClusterProvider : IClusterProvider
    {
        private readonly Func<IList<Uri>> replicasProvider;

        /// <param name="replicasProvider">An external delegate which will provides replica urls.</param>
        public AdHocClusterProvider(Func<IList<Uri>> replicasProvider)
        {
            this.replicasProvider = replicasProvider;
        }

        /// <inheritdoc />
        public IList<Uri> GetCluster() => replicasProvider();
    }
}