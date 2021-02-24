using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Misc;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class StatisticsHistory
    {
        private Statistic? clusterStatistic;
        private readonly Dictionary<Uri, Statistic> replicasHistoryStatistics 
            = new Dictionary<Uri, Statistic>();

        public Statistic? GetForReplica(Uri replica)
        {
            return replicasHistoryStatistics.TryGetValue(replica, out var statistic) 
                ? statistic 
                : default(Statistic?);
        }

        public Statistic? GetForCluster() =>
            clusterStatistic;

        public void Update(StatisticSnapshot snapshot)
        {
            clusterStatistic = snapshot.Cluster;
            
            foreach (var (replica, statistic) in snapshot.Replicas)
                replicasHistoryStatistics[replica] = statistic;
        }
    }
}