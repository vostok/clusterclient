using System;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class ClusterState
    {
        private readonly RelativeWeightSettings settings;
        
        public AtomicBoolean IsUpdatingNow { get; }
        public DateTime LastUpdateTimestamp { get;  set; }
        public ITimeProvider TimeProvider { get; }
        public IRelativeWeightCalculator RelativeWeightCalculator { get; }
        public IRawClusterStatistic CurrentStatistic { get; private set; }
        public IStatisticHistory StatisticHistory { get; private set; }
        public IWeights Weights { get; }
        
        public ClusterState(
            RelativeWeightSettings settings,
            IRelativeWeightCalculator relativeWeightCalculator = null,
            IRawClusterStatistic rawClusterStatistic = null,
            ITimeProvider timeProvider = null,
            IStatisticHistory statisticHistory = null, 
            IWeights weights = null)
        {
            this.settings = settings;
            
            IsUpdatingNow = new AtomicBoolean(false);
            TimeProvider = timeProvider ?? new TimeProvider();
            RelativeWeightCalculator = relativeWeightCalculator ?? new RelativeWeightCalculator(settings);
            Weights = weights ?? new Weights(settings);
            CurrentStatistic = rawClusterStatistic ?? new RawClusterStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);
            StatisticHistory = statisticHistory ?? new StatisticsHistory(settings.StatisticTTL);

            LastUpdateTimestamp = TimeProvider.GetCurrentTime();
        }

        public IRawClusterStatistic SwapToNewRawStatistic()
        {
            var previousRawStatistic = CurrentStatistic;

            CurrentStatistic = new RawClusterStatistic(settings.StatisticSmoothingConstant, settings.PenaltyMultiplier);

            return previousRawStatistic;
        }
    }
}