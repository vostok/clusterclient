using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represents an HTTP response from server.
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    [PublicAPI]
    public class Response : IDisposable
    {
        private readonly Content content;
        private readonly Headers headers;
        private readonly Stream stream;

        public Response(
            ResponseCode code,
            [CanBeNull] Content content = null,
            [CanBeNull] Headers headers = null,
            [CanBeNull] Stream stream = null)
        {
            if (content != null && stream != null)
                throw new ArgumentException("A response can't have both buffered content and a body stream.");

            Code = code;
            this.content = content;
            this.headers = headers;
            this.stream = stream;
        }

        /// <summary>
        /// Returns response code.
        /// </summary>
        public ResponseCode Code { get; }

        /// <summary>
        /// Returns response body content or an empty content if there is none.
        /// </summary>
        [NotNull]
        public Content Content => content ?? Content.Empty;

        /// <summary>
        /// Returns response headers or empty headers if there are none.
        /// </summary>
        [NotNull]
        public Headers Headers => headers ?? Headers.Empty;

        /// <summary>
        /// Returns response stream or an empty stream if there is none.
        /// </summary>
        [NotNull]
        public Stream Stream => stream ?? Stream.Null;

        /// <summary>
        /// Returns true if this response has buffered content, or false otherwise.
        /// </summary>
        public bool HasContent => content != null;

        /// <summary>
        /// Returns true if this response has any headers, or false otherwise.
        /// </summary>
        public bool HasHeaders => headers != null;

        /// <summary>
        /// Returns true if this response has response body stream, or false otherwise.
        /// </summary>
        public bool HasStream => stream != null;

        /// <summary>
        /// Returns <c>true</c> if <see cref="Code"/> belongs to 2xx range (see <see cref="ResponseCodeExtensions.IsSuccessful"/>), or false otherwise.
        /// </summary>
        public bool IsSuccessful => Code.IsSuccessful();

        public void Dispose()
        {
            stream?.Dispose();
        }

        /// <summary>
        /// <para>Produces a new <see cref="Response"/> instance where the header with given name will have given value.</para>
        /// <para>See <see cref="Headers"/> class documentation for details.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>A new <see cref="Response"/> object with updated headers.</returns>
        [Pure]
        [NotNull]
        public Response WithHeader([NotNull] string name, [NotNull] string value)
        {
            return new Response(Code, content, Headers.Set(name, value), stream);
        }

        /// <summary>
        /// <para>Produces a new <see cref="Response"/> instance where the header with given name will have given value.</para>
        /// <para>See <see cref="Headers"/> class documentation for details.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>A new <see cref="Response"/> object with updated headers.</returns>
        [Pure]
        [NotNull]
        public Response WithHeader<T>([NotNull] string name, [NotNull] T value)
        {
            return WithHeader(name, value.ToString());
        }

        /// <summary>
        /// <para>Produces a new <see cref="Response"/> instance where the header with given name will not exist.</para>
        /// <para>See <see cref="Headers"/> class documentation for details.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <returns>A new <see cref="Response"/> object with updated headers.</returns>
        [Pure]
        [NotNull]
        public Response RemoveHeader([NotNull] string name)
        {
            var newHeaders = Headers.Remove(name);

            if (ReferenceEquals(newHeaders, Headers))
                return this;

            return new Response(Code, content, newHeaders, stream);
        }

        /// <summary>
        /// Produces a new <see cref="Response"/> instance with given body content. Current instance is not modified.
        /// </summary>
        /// <returns>A new <see cref="Response"/> object with updated content.</returns>
        [Pure]
        [NotNull]
        public Response WithContent([NotNull] Content newContent)
        {
            return new Response(Code, newContent, headers, stream);
        }

        /// <summary>
        /// Produces a new <see cref="Response"/> instance with given body stream. Current instance is not modified.
        /// </summary>
        /// <returns>A new <see cref="Response"/> object with updated stream.</returns>
        [Pure]
        [NotNull]
        public Response WithStream([NotNull] Stream newStream)
        {
            return new Response(Code, content, headers, newStream);
        }

        /// <summary>
        /// Throws a <see cref="ClusterClientException"/> if <see cref="Code"/> doesn't belong to 2xx range (see <see cref="ResponseCodeExtensions.IsSuccessful"/>)
        /// </summary>
        public Response EnsureSuccessStatusCode()
        {
            if (!IsSuccessful)
                throw new ClusterClientException($"Response status code '{Code}' indicates unsuccessful outcome.");

            return this;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        [PublicAPI]
        public string ToString(bool includeHeaders)
        {
            var builder = new StringBuilder();

            builder.Append((int) Code);
            builder.Append(" ");
            builder.Append(Code);

            if (includeHeaders && headers != null && headers.Count > 0)
            {
                builder.AppendLine();
                builder.Append(headers);
            }

            return builder.ToString();
        }
    }
}
