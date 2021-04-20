using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class RawClusterStatistic : IRawClusterStatistic
    {
        private readonly TimeSpan smoothingConstant;
        private readonly int penaltyMultiplier;

        private readonly StatisticBucket clusterStatistic;
        private readonly ConcurrentDictionary<Uri, StatisticBucket> replicasStatistic;

        public RawClusterStatistic(TimeSpan smoothingConstant, int penaltyMultiplier)
        {
            this.smoothingConstant = smoothingConstant;
            this.penaltyMultiplier = penaltyMultiplier;

            clusterStatistic = new StatisticBucket();
            replicasStatistic = new ConcurrentDictionary<Uri, StatisticBucket>();
        }

        public void Report(ReplicaResult result)
        { 
            clusterStatistic
                .Report(result);
            replicasStatistic
                .GetOrAdd(result.Replica, new StatisticBucket())
                .Report(result);
        }

        public AggregatedClusterStatistic GetPenalizedAndSmoothedStatistic(DateTime currentTime, AggregatedClusterStatistic previous)
        {
            var penalty = CalculatePenalty();

            var smoothedAggregatedClusterStatistic = clusterStatistic
                .Penalize(penalty)
                .ObserveSmoothed(currentTime, smoothingConstant, previous?.Cluster);
            
            var replicasSmoothedAggregatedStatistic = new Dictionary<Uri, AggregatedStatistic>(replicasStatistic.Count);
            foreach (var (replica, statisticBucket) in replicasStatistic)
            {
                var replicaSmoothedStatistic = statisticBucket
                    .Penalize(penalty)
                    .ObserveSmoothed(currentTime, smoothingConstant, GetReplicaStatisticHistory(replica));
                replicasSmoothedAggregatedStatistic.Add(replica, replicaSmoothedStatistic);
            }
            return new AggregatedClusterStatistic(smoothedAggregatedClusterStatistic, replicasSmoothedAggregatedStatistic);

            AggregatedStatistic? GetReplicaStatisticHistory(Uri replica)
            {
                if (previous == null || !previous.Replicas.TryGetValue(replica, out var statistic))
                    return null;
                return statistic;
            }
        }

        internal double CalculatePenalty()
        {
            var globalStat = clusterStatistic.Observe(DateTime.UtcNow);
            return globalStat.Mean + globalStat.StdDev * penaltyMultiplier;
        }
    }
}