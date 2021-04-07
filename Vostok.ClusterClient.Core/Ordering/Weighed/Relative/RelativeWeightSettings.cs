using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    [PublicAPI]
    public class RelativeWeightSettings
    {
        public int PenaltyMultiplier = 100;
        public double MinWeight = 0.001;
        public double InitialWeight = 0.5;
        public double Sensitivity = 4;
        public double RegenerationRatePerMinute = 0.05;
        public TimeSpan WeightUpdatePeriod = TimeSpan.FromSeconds(3);
        public TimeSpan WeightsTTL = TimeSpan.FromMinutes(10);
        public TimeSpan RegenerationLag = TimeSpan.FromMinutes(3);
        public TimeSpan StatisticTTL = TimeSpan.FromMinutes(30);
        public TimeSpan StatisticSmoothingConstant = TimeSpan.FromSeconds(5);
        public TimeSpan WeightsDownSmoothingConstant = TimeSpan.FromSeconds(3);
        public TimeSpan WeightsRaiseSmoothingConstant = TimeSpan.FromMinutes(1);
    }
}