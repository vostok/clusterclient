using System;
using System.IO;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Model
{
    /// <inheritdoc />
    [PublicAPI]
    public class StreamContent : IStreamContent
    {
        /// <param name="stream">Body stream.</param>
        /// <param name="length">Content-length.</param>
        public StreamContent(Stream stream, long? length = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
                throw new ArgumentException("Request body stream must be readable.", nameof(stream));

            if (length.HasValue && length < 0)
                throw new ArgumentException($"Request body length must not be negative, but got '{length}'.", nameof(length));

            Stream = stream;
            Length = length;
        }

        /// <inheritdoc />
        public Stream Stream { get; }

        /// <inheritdoc />
        public long? Length { get; }
    }
}