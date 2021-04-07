using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal readonly struct Statistic
    {
        public readonly double StdDev;
        public readonly double Mean;
        public readonly DateTime Timestamp;

        public Statistic(double stdDev, double mean, DateTime timestamp)
        {
            StdDev = stdDev;
            Mean = mean;
            Timestamp = timestamp;
        }

        public bool IsZero() =>
            StdDev < double.Epsilon && Mean < double.Epsilon;

        public bool Equals(Statistic other) =>
            StdDev.Equals(other.StdDev) && Mean.Equals(other.Mean) && Timestamp.Equals(other.Timestamp);

        public override bool Equals(object obj) =>
            obj is Statistic other && Equals(other);

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