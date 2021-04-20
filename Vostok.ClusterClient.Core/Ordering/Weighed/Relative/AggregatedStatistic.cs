using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal readonly struct AggregatedStatistic
    {
        public readonly double StdDev;
        public readonly double Mean;
        public readonly DateTime Timestamp;

        public AggregatedStatistic(double stdDev, double mean, DateTime timestamp)
        {
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

            return new AggregatedStatistic(smoothedStdDev, smoothedMean, Timestamp);
        }

        public bool Equals(AggregatedStatistic other) =>
            StdDev.Equals(other.StdDev) && Mean.Equals(other.Mean) && Timestamp.Equals(other.Timestamp);

        public override bool Equals(object obj) =>
            obj is AggregatedStatistic other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StdDev.GetHashCode();
                hashCode = (hashCode * 397) ^ Mean.GetHashCode();
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                return hashCode;
            }
        }
    }
}