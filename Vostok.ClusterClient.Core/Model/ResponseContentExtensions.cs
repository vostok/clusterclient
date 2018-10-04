using System;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// A set of <see cref="Content"/>-related extensions for <see cref="Response"/>.
    /// </summary>
    [PublicAPI]
    public static class ResponseContentExtensions
    {
        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given <paramref name="buffer"/>.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] byte[] buffer)
        {
            return response.WithContent(new Content(buffer));
        }

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given <paramref name="buffer"/> at given coordinates.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] byte[] buffer, int offset, int length)
        {
            return response.WithContent(new Content(buffer, offset, length));
        }

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given byte array segment.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, ArraySegment<byte> content)
        {
            return response.WithContent(new Content(content));
        }

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given string encoded by given <paramref name="encoding"/>.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] string content, [NotNull] Encoding encoding)
        {
            return response.WithContent(new Content(encoding.GetBytes(content)));
        }

        /// <summary>
        /// Returns a new <see cref="Response"/> instance with content from given string encoded by <see cref="UTF8Encoding"/>.
        /// </summary>
        [Pure]
        [NotNull]
        public static Response WithContent([NotNull] this Response response, [NotNull] string content)
        {
            return WithContent(response, content, Encoding.UTF8);
        }
    }
}