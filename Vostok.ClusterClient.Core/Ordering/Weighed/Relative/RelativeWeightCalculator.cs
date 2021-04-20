using System;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class RelativeWeightCalculator : IRelativeWeightCalculator
    {
        private readonly RelativeWeightSettings settings;

        public RelativeWeightCalculator(RelativeWeightSettings settings) =>
            this.settings = settings;

        public Weight Calculate(in AggregatedStatistic clusterAggregatedStatistic, in AggregatedStatistic replicaAggregatedStatistic, in Weight previousWeight)
        {
            var newWeight = EnforceRelativeLimits(
                WeighingHelper
                    .ComputeWeight(replicaAggregatedStatistic.Mean, replicaAggregatedStatistic.StdDev, clusterAggregatedStatistic.Mean, clusterAggregatedStatistic.StdDev, settings.Sensitivity));
            var smoothingConstant = newWeight > previousWeight.Value
                ? settings.WeightsRaiseSmoothingConstant
                : settings.WeightsDownSmoothingConstant;
            var smoothedWeight = SmoothingHelper
                .SmoothValue(newWeight, previousWeight.Value, clusterAggregatedStatistic.Timestamp, previousWeight.Timestamp, smoothingConstant);
            return new Weight(smoothedWeight, replicaAggregatedStatistic.Timestamp);

            double EnforceRelativeLimits(double rowWeight) =>
                Math.Min(1.0, Math.Max(settings.MinWeight, rowWeight));
        }
    }
}