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
            var weightByStatuses = clusterAggregatedStatistic.TotalCount / settings.WeightUpdatePeriod.TotalSeconds <= settings.WeightByStatusesRpsThreshold;
            var rawWeight = weightByStatuses
                ? WeighingHelper.ComputeWeightByStatuses(replicaAggregatedStatistic.TotalCount, replicaAggregatedStatistic.ErrorFraction, clusterAggregatedStatistic.TotalCount, clusterAggregatedStatistic.ErrorFraction, settings.Sensitivity)
                : WeighingHelper.ComputeWeightByLatency(replicaAggregatedStatistic.Mean, replicaAggregatedStatistic.StdDev, clusterAggregatedStatistic.Mean, clusterAggregatedStatistic.StdDev, settings.Sensitivity);
            
            var newWeight = EnforceRelativeLimits(rawWeight);
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