using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ClusterStatistic
    {
        public readonly Statistic Cluster;
        public readonly IReadOnlyDictionary<Uri, Statistic> Replicas;

        public ClusterStatistic(
            Statistic cluster, 
            IReadOnlyDictionary<Uri, Statistic> replicas)
        {
            Cluster = cluster;
            Replicas = replicas;
        }
    }
}