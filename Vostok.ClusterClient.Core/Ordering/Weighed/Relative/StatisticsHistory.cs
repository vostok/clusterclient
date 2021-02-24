using System;
using System.Collections.Generic;

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
            
            foreach (var replicaSnapshot in snapshot.Replicas)
                replicasHistoryStatistics[replicaSnapshot.Key] = replicaSnapshot.Value;
        }
    }
}