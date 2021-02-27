using System;
using System.Linq;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ClusterState
    {
        private readonly Func<IActiveStatistic> activeStatisticFactory;
        private readonly IStatisticHistory statisticHistory;

        public DateTime LastUpdateTimestamp { get; private set; } = DateTime.UtcNow;
        public IActiveStatistic CurrentStatistic { get; private set; }
        public IWeights Weights { get; }

        public ClusterState(
            RelativeWeightSettings settings, 
            Func<IActiveStatistic> activeStatisticFactory = null, 
            IStatisticHistory statisticHistory = null, 
            IWeights weights = null)
        {
            this.activeStatisticFactory = activeStatisticFactory ?? CreateDefault;
            this.statisticHistory = statisticHistory ?? new StatisticsHistory();
            Weights = weights ?? new Weights(settings.WeightsTTL);
            CurrentStatistic = this.activeStatisticFactory();

            IActiveStatistic CreateDefault() =>
                new ActiveStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);
        }

        public StatisticSnapshot ExchangeStatistic(DateTime currentTimestamp)
        {
            LastUpdateTimestamp = currentTimestamp;

            var previousActiveStatistic = CurrentStatistic;
            CurrentStatistic = activeStatisticFactory();

            var penalty = previousActiveStatistic.CalculatePenalty();
            var clusterStatistic = previousActiveStatistic
                .ObserveCluster(currentTimestamp, penalty, statisticHistory.GetForCluster());
            var replicasStatistic = previousActiveStatistic
                .ObserveReplicas(currentTimestamp, penalty, uri => statisticHistory.GetForReplica(uri))
                .ToDictionary(t => t.Replica, t => t.Statistic);
            var snapshot = new StatisticSnapshot(clusterStatistic, replicasStatistic);
            
            statisticHistory.Update(snapshot);
            
            return snapshot;
        }
    }
}