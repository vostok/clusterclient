using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// <para>Represents an HTTP request (method, url, headers and body content).</para>
    /// <para>Every <see cref="Request"/> object is effectively immutable. Any modifications produce a new object.</para>
    /// <para>Look at <see cref="RequestUrlBuilder"/> to quickly build request urls with collection initializer syntax.</para>
    /// <para>Look at <see cref="RequestHeadersExtensions"/> to quickly set common headers.</para>
    /// <para>Look at <see cref="RequestContentExtensions"/> to quickly add content to requests.</para>
    /// <para>Look at <see cref="RequestQueryExtensions"/> to quickly add query parameters to requests.</para>
    /// </summary>
    public class Request
    {
        public Request(
            [NotNull] string method,
            [NotNull] Uri url,
            [CanBeNull] Content content = null,
            [CanBeNull] Headers headers = null)
            : this(method, url, null, content, headers)
        {
        }

        public Request(
            [NotNull] string method,
            [NotNull] Uri url,
            [NotNull] IStreamContent content,
            [CanBeNull] Headers headers = null)
            : this(method, url, content, null, headers)
        {
        }

        private Request(
            [NotNull] string method,
            [NotNull] Uri url,
            [CanBeNull] IStreamContent streamContent,
            [CanBeNull] Content content,
            [CanBeNull] Headers headers)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            StreamContent = streamContent;
            Content = content;
            Headers = headers;
        }

        /// <summary>
        /// Returns request method (one of <see cref="RequestMethods"/>).
        /// </summary>
        [NotNull]
        public string Method { get; }

        /// <summary>
        /// Returns request url.
        /// </summary>
        [NotNull]
        public Uri Url { get; }

        /// <summary>
        /// Returns request content or <c>null</c> if there is none.
        /// </summary>
        [CanBeNull]
        public Content Content { get; }

        /// <summary>
        /// Returns request stream content or <c>null</c> if there is none.
        /// </summary>
        [CanBeNull]
        public IStreamContent StreamContent { get; }

        /// <summary>
        /// Returns request headers or <c>null</c> if there are none.
        /// </summary>
        [CanBeNull]
        public Headers Headers { get; }

        /// <summary>
        /// <para>Returns true if current <see cref="Request"/> instance is valid, or false otherwise.</para>
        /// <para><see cref="Validate"/> method can be used to obtain error messages.</para>
        /// </summary>
        public bool IsValid => !Validate().Any();

        /// <summary>
        /// Returns true if current instance has either non-null <see cref="Content"/> or <see cref="StreamContent"/>.
        /// </summary>
        public bool HasBody => Content != null || StreamContent != null;

        /// <summary>
        /// Produces a new <see cref="Request"/> instance with given url. Current instance is not modified.
        /// </summary>
        /// <returns>A new <see cref="Request"/> object with updated url.</returns>
        [Pure]
        [NotNull]
        public Request WithUrl([NotNull] string url) =>
            WithUrl(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Produces a new <see cref="Request"/> instance with given url. Current instance is not modified.
        /// </summary>
        /// <returns>A new <see cref="Request"/> object with updated url.</returns>
        [Pure]
        [NotNull]
        public Request WithUrl([NotNull] Uri url) =>
            new Request(Method, url, StreamContent, Content, Headers);

        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance with given body content. Current instance is not modified.</para>
        /// <para>If current instance contains a non-null <see cref="StreamContent"/> property, it will be discarded in new instance.</para>
        /// </summary>
        /// <returns>A new <see cref="Request"/> object with updated content.</returns>
        [Pure]
        [NotNull]
        public Request WithContent([NotNull] Content content) =>
            new Request(Method, Url, content, (Headers ?? Headers.Empty).Set(HeaderNames.ContentLength, content.Length));

        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance with given body stream content. Current instance is not modified.</para>
        /// <para>If current instance contains a non-null <see cref="Content"/> property, it will be discarded in new instance.</para>
        /// </summary>
        /// <returns>A new <see cref="Request"/> object with updated content.</returns>
        [Pure]
        [NotNull]
        public Request WithContent([NotNull] IStreamContent content) =>
            new Request(Method, Url, content, content.Length.HasValue ? (Headers ?? Headers.Empty).Set(HeaderNames.ContentLength, content.Length.Value) : Headers);

        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance where the header with given name will have given value.</para>
        /// <para>See <see cref="Headers"/> class documentation for details.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value. ToString() is used to obtain string value.</param>
        /// <returns>A new <see cref="Request"/> object with updated headers.</returns>
        [Pure]
        [NotNull]
        public Request WithHeader<T>([NotNull] string name, [NotNull] T value) =>
            WithHeader(name, value.ToString());

        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance where the header with given name will have given value.</para>
        /// <para>See <see cref="Headers"/> class documentation for details.</para>
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        /// <returns>A new <see cref="Request"/> object with updated headers.</returns>
        [Pure]
        [NotNull]
        public Request WithHeader([NotNull] string name, [NotNull] string value) =>
            new Request(Method, Url, StreamContent, Content, (Headers ?? Headers.Empty).Set(name, value));

        /// <summary>
        /// <para>Produces a new <see cref="Request"/> instance with headers substituted by given collection.</para>
        /// <para>See <see cref="Headers"/> class documentation for details.</para>
        /// </summary>
        /// <returns>A new <see cref="Request"/> object with given headers.</returns>
        [Pure]
        [NotNull]
        public Request WithHeaders([NotNull] Headers headers) =>
            new Request(Method, Url, StreamContent, Content, headers);

        public override string ToString() => ToString(false, false);

        public string ToString(bool includeQuery, bool includeHeaders)
        {
            var builder = new StringBuilder();

            builder.Append(Method);
            builder.Append(" ");

            var urlString = Url.ToString();

            if (!includeQuery)
            {
                var queryBeginning = urlString.IndexOf("?", StringComparison.Ordinal);
                if (queryBeginning >= 0)
                    urlString = urlString.Substring(0, queryBeginning);
            }

            builder.Append(urlString);

            if (includeHeaders && Headers != null && Headers.Count > 0)
            {
                builder.AppendLine();
                builder.Append(Headers);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns all validation errors for this <see cref="Request"/> instance. An empty sequence is returned for a valid request.
        /// </summary>
        public IEnumerable<string> Validate()
        {
            if (!RequestMethods.All.Contains(Method))
                yield return $"Request method has unsupported value '{Method}'.";

            if (Url.IsAbsoluteUri)
            {
                var scheme = Url.Scheme;
                if (scheme != Uri.UriSchemeHttp && scheme != Uri.UriSchemeHttps)
                    yield return $"Request url has unsupported scheme '{scheme}'. Only http and https schemes are allowed.";
            }

            if (HasBody && (Method == RequestMethods.Get || Method == RequestMethods.Head))
                yield return $"Sending a body is not allowed with {Method} requests.";
        }

        #region Factory methods

        /// <summary>
        /// Creates a new request with <c>GET</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Get([NotNull] Uri url) =>
            new Request(RequestMethods.Get, url);

        /// <summary>
        /// Creates a new request with <c>GET</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Get([NotNull] string url) =>
            Get(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>POST</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Post([NotNull] Uri url) =>
            new Request(RequestMethods.Post, url);

        /// <summary>
        /// Creates a new request with <c>POST</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Post([NotNull] string url) =>
            Post(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>PUT</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Put([NotNull] Uri url) =>
            new Request(RequestMethods.Put, url);

        /// <summary>
        /// Creates a new request with <c>PUT</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Put([NotNull] string url) =>
            Put(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>HEAD</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Head([NotNull] Uri url) =>
            new Request(RequestMethods.Head, url);

        /// <summary>
        /// Creates a new request with <c>HEAD</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Head([NotNull] string url) =>
            Head(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>PATCH</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Patch([NotNull] Uri url) =>
            new Request(RequestMethods.Patch, url);

        /// <summary>
        /// Creates a new request with <c>PATCH</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Patch([NotNull] string url) =>
            Patch(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>DELETE</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Delete([NotNull] Uri url) =>
            new Request(RequestMethods.Delete, url);

        /// <summary>
        /// Creates a new request with <c>DELETE</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Delete([NotNull] string url) =>
            Delete(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>OPTIONS</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Options([NotNull] Uri url) =>
            new Request(RequestMethods.Options, url);

        /// <summary>
        /// Creates a new request with <c>OPTIONS</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Options([NotNull] string url) =>
            Options(new Uri(url, UriKind.RelativeOrAbsolute));

        /// <summary>
        /// Creates a new request with <c>TRACE</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Trace([NotNull] Uri url) =>
            new Request(RequestMethods.Trace, url);

        /// <summary>
        /// Creates a new request with <c>TRACE</c> method and given <paramref name="url"/>.
        /// </summary>
        [NotNull]
        public static Request Trace([NotNull] string url) =>
            Trace(new Uri(url, UriKind.RelativeOrAbsolute));

        #endregion
    }
}