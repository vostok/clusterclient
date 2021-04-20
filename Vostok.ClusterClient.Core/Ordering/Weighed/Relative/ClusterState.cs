using System;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ClusterState
    {
        private readonly Func<IRawClusterStatistic> activeStatisticFactory;
        private readonly IStatisticHistory statisticHistory;

        public AtomicBoolean IsUpdatingNow { get; }
        public DateTime LastUpdateTimestamp { get; private set; }
        public IRawClusterStatistic CurrentStatistic { get; private set; }
        public IWeights Weights { get; }

        public ClusterState(
            RelativeWeightSettings settings, 
            // CR(m_kiskachi) Интерфейс конструктора не должен подстраиваться под тесты.
            Func<IRawClusterStatistic> activeStatisticFactory = null, 
            IStatisticHistory statisticHistory = null, 
            IWeights weights = null)
        {
            this.activeStatisticFactory = activeStatisticFactory ?? CreateDefault;
            this.statisticHistory = statisticHistory ?? new StatisticsHistory(settings.StatisticTTL);

            IsUpdatingNow = new AtomicBoolean(false);
            LastUpdateTimestamp = DateTime.UtcNow;
            Weights = weights ?? new Weights(settings);
            CurrentStatistic = this.activeStatisticFactory();

            IRawClusterStatistic CreateDefault() =>
                new RawClusterStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);
        }

        public AggregatedClusterStatistic FlushCurrentRawStatisticToHistory(DateTime currentTimestamp)
        {
            LastUpdateTimestamp = currentTimestamp;

            var previousActiveStatistic = CurrentStatistic;
            CurrentStatistic = activeStatisticFactory();

            var clusterStatistic = previousActiveStatistic
                .GetPenalizedAndSmoothedStatistic(currentTimestamp, statisticHistory.Get());
            
            statisticHistory.Update(clusterStatistic);
            
            return clusterStatistic;
        }
    }
}