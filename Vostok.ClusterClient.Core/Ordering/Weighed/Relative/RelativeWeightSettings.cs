using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    [PublicAPI]
    public class RelativeWeightSettings
    {
        public int PenaltyMultiplier = 100;
        public double MinWeight = 0.005;
        public double InitialWeight = 1.0d;
        public double Sensitivity = 4;
        public TimeSpan WeightUpdatePeriod = TimeSpan.FromSeconds(3);
        public TimeSpan WeightsTTL = TimeSpan.FromMinutes(1);
        public TimeSpan StatisticSmoothingConstant = TimeSpan.FromSeconds(5);
        public TimeSpan WeightsDownSmoothingConstant = TimeSpan.FromSeconds(3);
        public TimeSpan WeightsRaiseSmoothingConstant = TimeSpan.FromMinutes(1);
    }
}