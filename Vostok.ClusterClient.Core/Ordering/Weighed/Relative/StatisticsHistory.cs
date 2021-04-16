using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class StatisticsHistory : IStatisticHistory
    {
        private readonly TimeSpan statisticTtl;
        private ClusterStatistic currentHistory;

        public StatisticsHistory(TimeSpan statisticTtl)
        {
            this.statisticTtl = statisticTtl;
        }

        public ClusterStatistic Get() =>
            currentHistory != null
                ? new ClusterStatistic(currentHistory.Cluster, currentHistory.Replicas) 
                : null;

        public void Update(ClusterStatistic snapshot)
        {
            if (currentHistory == null)
            {
                // CR(m_kiskachi) Зачем создавать новый?
                currentHistory = new ClusterStatistic(snapshot.Cluster, snapshot.Replicas);
                return;
            }

            var newReplicas = new HashSet<Uri>(snapshot.Replicas.Keys);
            var replicasUpdatedHistory = new Dictionary<Uri, Statistic>(snapshot.Replicas.Count);
            foreach (var (currentReplica, currentStatistic) in currentHistory.Replicas)
            {
                if (snapshot.Replicas.ContainsKey(currentReplica))
                {
                    replicasUpdatedHistory[currentReplica] = snapshot.Replicas[currentReplica];
                    newReplicas.Remove(currentReplica);
                    continue;
                }

                // CR(m_kiskachi) Можно закешировать DateTime.UtcNow для цикла, чтобы вычищение по TTL происходило 
                // CR(m_kiskachi) как бы в один момент времени, а не размазывалось на разные доли секунд.
                if (DateTime.UtcNow - currentStatistic.Timestamp < statisticTtl)
                    replicasUpdatedHistory[currentReplica] = currentStatistic;
            }

            foreach (var newReplica in newReplicas)
                replicasUpdatedHistory[newReplica] = snapshot.Replicas[newReplica];

            currentHistory = new ClusterStatistic(snapshot.Cluster, replicasUpdatedHistory);
        }
    }
}