using System;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// Represents an HTTP header in the form of a simple string key-value pair.
    /// </summary>
    [PublicAPI]
    public struct Header : IEquatable<Header>
    {
        private string name;
        private string value;

        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        public Header([NotNull] string name, [NotNull] string value)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns header name.
        /// </summary>
        [NotNull]
        public string Name => name ?? string.Empty;

        /// <summary>
        /// Returns header value.
        /// </summary>
        [NotNull]
        public string Value => value ?? string.Empty;

        /// <returns>String representation of header in "<see cref="Name"/>: <see cref="Value"/>" format.</returns>
        public override string ToString()
        {
            return Name + ": " + Value;
        }

        #region Equality members

        /// <summary>
        /// Compares two <see cref="Headers"/> instances.
        /// </summary>
        public bool Equals(Header other)
        {
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Header) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
            => unchecked(StringComparer.OrdinalIgnoreCase.GetHashCode(Name) * 397) ^ Value.GetHashCode();

        #endregion
    }
}