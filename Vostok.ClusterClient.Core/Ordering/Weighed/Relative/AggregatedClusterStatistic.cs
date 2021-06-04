using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class AggregatedClusterStatistic
    {
        public readonly AggregatedStatistic Cluster;
        public readonly IReadOnlyDictionary<Uri, AggregatedStatistic> Replicas;

        public AggregatedClusterStatistic(
            AggregatedStatistic cluster, 
            IReadOnlyDictionary<Uri, AggregatedStatistic> replicas)
        {
            Cluster = cluster;
            Replicas = replicas;
        }
    }
}