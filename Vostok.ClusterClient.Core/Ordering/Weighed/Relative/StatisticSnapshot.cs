using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class StatisticSnapshot
    {
        public readonly Statistic Cluster;
        public readonly IReadOnlyDictionary<Uri, Statistic> Replicas;

        public StatisticSnapshot(
            Statistic cluster, 
            IReadOnlyDictionary<Uri, Statistic> replicas)
        {
            Cluster = cluster;
            Replicas = replicas;
        }
    }
}