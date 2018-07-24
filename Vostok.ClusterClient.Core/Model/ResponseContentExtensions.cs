using System;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    public static class ResponseContentExtensions
    {
        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given <paramref name="buffer"/>.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] byte[] buffer) =>
            response.WithContent(new Content(buffer));

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given <paramref name="buffer"/> at given coordinates.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] byte[] buffer, int offset, int length) =>
            response.WithContent(new Content(buffer, offset, length));

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given byte array segment.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, ArraySegment<byte> content) =>
            response.WithContent(new Content(content));

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given string encoded by given <paramref name="encoding"/>.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] string content, [NotNull] Encoding encoding) =>
            response.WithContent(new Content(encoding.GetBytes(content)));

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given string encoded by <see cref="UTF8Encoding"/>.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] string content) =>
            WithContent(response, content, Encoding.UTF8);
    }
}