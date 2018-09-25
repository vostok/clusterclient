using System;
using System.Globalization;
using System.Linq;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represent a header Quality Value defined in RFC 7231 5.3.1.
    /// </summary>
    public class HeaderValueWithQuality
    {
        /// <param name="value">Header value.</param>
        /// <param name="quality">Header quality. Must be in [0; 1] range.</param>
        public HeaderValueWithQuality(string value, decimal quality = 1m)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty", nameof(value));

            if (value.Contains(',') || value.Contains(';'))
                throw new ArgumentException("Value cannot contain delimiters", nameof(value));

            if (quality > 1m || quality < 0m)
                throw new ArgumentException("Quality must be in range [0; 1]", nameof(quality));

            Value = value;
            Quality = quality;
        }

        /// <summary>
        /// Header value.
        /// </summary>
        public string Value { get; }
        
        /// <summary>
        /// Quality of this value.
        /// </summary>
        public decimal Quality { get; }

        /// <returns>Header value with quality string representation in "<see cref="Value"/>;q=<see cref="Quality"/>" format.</returns>
        public override string ToString() =>
            Quality < 1m ? $"{Value};q={Quality.ToString("0.###", NumberFormatInfo.InvariantInfo)}" : Value;

        #region Equality members 

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is HeaderValueWithQuality quality && Equals(quality);
        }

        /// <summary>
        /// Compares two <see cref="HeaderValueWithQuality"/>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(HeaderValueWithQuality other) =>
            string.Equals(Value, other.Value, StringComparison.InvariantCultureIgnoreCase) && Quality == other.Quality;

        /// <inheritdoc />
        public override int GetHashCode() =>
            unchecked (((Value?.GetHashCode() ?? 0) * 397) ^ Quality.GetHashCode());

        #endregion
    }
}