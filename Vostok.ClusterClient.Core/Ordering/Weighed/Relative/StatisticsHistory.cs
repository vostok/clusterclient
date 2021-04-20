using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class StatisticsHistory : IStatisticHistory
    {
        private readonly TimeSpan statisticTtl;
        private AggregatedClusterStatistic currentHistory;

        public StatisticsHistory(TimeSpan statisticTtl)
        {
            this.statisticTtl = statisticTtl;
        }

        public AggregatedClusterStatistic Get() =>
            currentHistory != null
                ? new AggregatedClusterStatistic(currentHistory.Cluster, currentHistory.Replicas) 
                : null;

        public void Update(AggregatedClusterStatistic snapshot)
        {
            if (currentHistory == null)
            {
                currentHistory = snapshot;
                return;
            }

            var newReplicas = new HashSet<Uri>(snapshot.Replicas.Keys);
            var replicasUpdatedHistory = new Dictionary<Uri, AggregatedStatistic>(snapshot.Replicas.Count);
            var currentTime = DateTime.UtcNow;
            foreach (var (currentReplica, currentStatistic) in currentHistory.Replicas)
            {
                if (snapshot.Replicas.ContainsKey(currentReplica))
                {
                    replicasUpdatedHistory[currentReplica] = snapshot.Replicas[currentReplica];
                    newReplicas.Remove(currentReplica);
                    continue;
                }

                if (currentTime - currentStatistic.Timestamp < statisticTtl)
                    replicasUpdatedHistory[currentReplica] = currentStatistic;
            }

            foreach (var newReplica in newReplicas)
                replicasUpdatedHistory[newReplica] = snapshot.Replicas[newReplica];

            currentHistory = new AggregatedClusterStatistic(snapshot.Cluster, replicasUpdatedHistory);
        }
    }
}