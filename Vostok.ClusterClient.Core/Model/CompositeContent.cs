using System;
using System.Linq;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// Represents a buffered in-memory content transferred to server, composed of several buffer segments.
    /// </summary>
    [PublicAPI]
    public class CompositeContent
    {
        public CompositeContent([NotNull] Content[] parts)
        {
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));
            Length = parts.Sum(part => (long) part.Length);
        }

        /// <summary>
        /// Returns the buffered parts this content consists of.
        /// </summary>
        [NotNull]
        public Content[] Parts { get; }

        /// <summary>
        /// Returns the total length of the data present in all <see cref="Parts"/>.
        /// </summary>
        public long Length { get; }
    }
}