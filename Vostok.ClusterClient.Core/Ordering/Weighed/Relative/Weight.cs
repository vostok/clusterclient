using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal readonly struct Weight
    {
        public readonly double Value;
        public readonly DateTime Timestamp;

        public Weight(double value, DateTime timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }
    }
}