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

        public override string ToString() =>
            $"{Value:F3}, {Timestamp:G}";

        public bool Equals(Weight other) =>
            Value.Equals(other.Value) && Timestamp.Equals(other.Timestamp);

        public override bool Equals(object obj) =>
            obj is Weight other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ Timestamp.GetHashCode();
            }
        }
    }
}