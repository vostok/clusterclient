using System;
using System.IO;

namespace Vostok.ClusterClient.Core.Model
{
    public class StreamContent : IStreamContent
    {
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

        public Stream Stream { get; }

        public long? Length { get; }
    }
}