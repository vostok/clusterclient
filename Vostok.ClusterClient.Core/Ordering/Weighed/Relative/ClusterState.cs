using System;
using System.Linq;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ClusterState
    {
        private readonly RelativeWeightSettings settings;
        private readonly StatisticsHistory previousStatistic;

        public DateTime LastUpdateTimestamp { get; private set; } = DateTime.UtcNow;
        public ActiveStatistic CurrentStatistic { get; private set; }
        public Weights Weights { get; }
        public ClusterState(RelativeWeightSettings settings)
        {
            this.settings = settings;
            previousStatistic = new StatisticsHistory();

            Weights = new Weights();
            CurrentStatistic = new ActiveStatistic(
                settings.StatisticSmoothingConstant, 
                settings.PenaltyMultiplier);
        }

        public StatisticSnapshot ExchangeStatistic(DateTime currentTimestamp)
        {
            LastUpdateTimestamp = currentTimestamp;

            var current = CurrentStatistic;
            CurrentStatistic = new ActiveStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);

            var penalty = current.CalculatePenalty();
            var clusterStatistic = current
                .ObserveCluster(currentTimestamp, penalty, previousStatistic.GetForCluster());
            var replicasStatistic = current
                .ObserveReplicas(currentTimestamp, penalty, uri => previousStatistic.GetForReplica(uri))
                .ToDictionary(t => t.Replica, t => t.Statistic);

            return new StatisticSnapshot(clusterStatistic, replicasStatistic);
        }
    }
}