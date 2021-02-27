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
    }
}