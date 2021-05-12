using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal readonly struct AggregatedStatistic
    {
        public readonly double TotalCount;
        public readonly double ErrorFraction;
        public readonly double StdDev;
        public readonly double Mean;
        public readonly DateTime Timestamp;

        public AggregatedStatistic(double totalCount, double errorFraction, double stdDev, double mean, DateTime timestamp)
        {
            TotalCount = totalCount;
            ErrorFraction = errorFraction;
            StdDev = stdDev;
            Mean = mean;
            Timestamp = timestamp;
        }

        public AggregatedStatistic Smooth(AggregatedStatistic? previous, TimeSpan smoothingConstant)
        {
            if (!previous.HasValue)
                return this;

            var previousAggregatedStatistic = previous.Value;
            var smoothedStdDev = SmoothingHelper
                .SmoothValue(StdDev, previousAggregatedStatistic.StdDev, Timestamp, previousAggregatedStatistic.Timestamp, smoothingConstant);
            var smoothedMean = SmoothingHelper
                .SmoothValue(Mean, previousAggregatedStatistic.Mean, Timestamp, previousAggregatedStatistic.Timestamp, smoothingConstant);
            var smoothedTotal = SmoothingHelper
                .SmoothValue(TotalCount, previousAggregatedStatistic.TotalCount, Timestamp, previousAggregatedStatistic.Timestamp, smoothingConstant);
            var smoothedErrorFraction = SmoothingHelper
                .SmoothValue(ErrorFraction, previousAggregatedStatistic.ErrorFraction, Timestamp, previousAggregatedStatistic.Timestamp, smoothingConstant);
            return new AggregatedStatistic(smoothedTotal, smoothedErrorFraction, smoothedStdDev, smoothedMean, Timestamp);
        }

        public bool Equals(AggregatedStatistic other) =>
            StdDev.Equals(other.StdDev) &&
            Mean.Equals(other.Mean) &&
            Timestamp.Equals(other.Timestamp) &&
            TotalCount.Equals(other.TotalCount) &&
            ErrorFraction.Equals(other.ErrorFraction);

        public override bool Equals(object obj) =>
            obj is AggregatedStatistic other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StdDev.GetHashCode();
                hashCode = (hashCode * 397) ^ Mean.GetHashCode();
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ TotalCount.GetHashCode();
                hashCode = (hashCode * 397) ^ ErrorFraction.GetHashCode();
                return hashCode;
            }
        }
    }
}