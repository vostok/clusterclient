using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    [PublicAPI]
    public class RelativeWeightSettings
    {
        public int PenaltyMultiplier = 100;
        public double MinWeight = 0.001;
        public double InitialWeight = 1;
        public double Sensitivity = 4;
        public double RegenerationRatePerMinute = 0.05;
        public double WeightByStatusesRpsThreshold = 1;
        public TimeSpan WeightUpdatePeriod = TimeSpan.FromSeconds(10);
        public TimeSpan WeightsTTL = TimeSpan.FromMinutes(10);
        public TimeSpan RegenerationLag = TimeSpan.FromMinutes(1);
        public TimeSpan StatisticTTL = TimeSpan.FromMinutes(10);
        public TimeSpan StatisticSmoothingConstant = TimeSpan.FromSeconds(1);
        public TimeSpan WeightsDownSmoothingConstant = TimeSpan.FromSeconds(1);
        public TimeSpan WeightsRaiseSmoothingConstant = TimeSpan.FromMinutes(1);
    }
}