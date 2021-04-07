using System;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class RelativeWeightCalculator : IRelativeWeightCalculator
    {
        private readonly RelativeWeightSettings settings;

        public RelativeWeightCalculator(RelativeWeightSettings settings) =>
            this.settings = settings;

        public Weight Calculate(in Statistic clusterStatistic, in Statistic replicaStatistic, in Weight previousWeight)
        {
            var newWeight = EnforceRelativeLimits(
                WeighingHelper
                    .ComputeWeight(replicaStatistic.Mean, replicaStatistic.StdDev, clusterStatistic.Mean, clusterStatistic.StdDev, settings.Sensitivity));
            var smoothingConstant = newWeight > previousWeight.Value
                ? settings.WeightsRaiseSmoothingConstant
                : settings.WeightsDownSmoothingConstant;
            var smoothedWeight = SmoothingHelper
                .SmoothValue(newWeight, previousWeight.Value, clusterStatistic.Timestamp, previousWeight.Timestamp, smoothingConstant);
            return new Weight(smoothedWeight, replicaStatistic.Timestamp);

            double EnforceRelativeLimits(double rowWeight) =>
                Math.Min(1.0, Math.Max(settings.MinWeight, rowWeight));
        }
    }
}