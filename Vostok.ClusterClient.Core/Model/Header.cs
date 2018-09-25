using System;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represents an HTTP header in the form of a simple string key-value pair.
    /// </summary>
    [PublicAPI]
    public class Header : IEquatable<Header>
    {
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        public Header([NotNull] string name, [NotNull] string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Returns header name.
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Returns header value.
        /// </summary>
        [NotNull]
        public string Value { get; }

        /// <returns>String representation of header in "<see cref="Name"/>: <see cref="Value"/>" format.</returns>
        public override string ToString()
        {
            return Name + ": " + Value;
        }

        #region Equality members

        public bool Equals(Header other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Name, other.Name) && string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((Header) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode()*397) ^ Value.GetHashCode();
            }
        }

        #endregion
    }
}
