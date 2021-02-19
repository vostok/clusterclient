using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ClusterState
    {
        private readonly RelativeWeightSettings settings;

        public DateTime LastUpdateTimestamp = DateTime.UtcNow;
        public readonly Weights Weights;
        public readonly StatisticsHistory StatisticsHistory;
        
        public ActiveStatistic ActiveStatistic { get; private set; }

        public ClusterState(RelativeWeightSettings settings)
        {
            this.settings = settings;

            ActiveStatistic = new ActiveStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);
            Weights = new Weights();
            StatisticsHistory = new StatisticsHistory();
        }

        public ActiveStatistic ExchangeActiveStat()
        {
            var current = ActiveStatistic;
            ActiveStatistic = new ActiveStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);
            return current;
        }
    }
}