using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Strategies;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// <para>Contains the names of well-known common HTTP headers.</para>
    /// <para>Values are taken from corresponding RFC (https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html).</para>
    /// </summary>
    [PublicAPI]
    public static class HeaderNames
    {
        public const string Accept = "Accept";
        public const string AcceptCharset = "Accept-Charset";
        public const string AcceptEncoding = "Accept-Encoding";
        public const string AcceptLanguage = "Accept-Language";
        public const string AccessControlRequestMethod = "Access-Control-Request-Method";
        public const string AccessControlRequestHeader = "Access-Control-Request-Headers";
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        public const string AccessControlMaxAge = "Access-Control-Max-Age";
        public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";
        public const string Age = "Age";
        public const string Allow = "Allow";
        public const string Authorization = "Authorization";
        public const string CacheControl = "Cache-Control";
        public const string Cookie = "Cookie";
        public const string Connection = "Connection";
        public const string ContentDisposition = "Content-Disposition";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLength = "Content-Length";
        public const string ContentType = "Content-Type";
        public const string ContentLanguage = "Content-Language";
        public const string ContentLocation = "Content-Location";
        public const string ContentRange = "Content-Range";
        public const string ContentMD5 = "Content-MD5";
        public const string Date = "Date";
        public const string ETag = "ETag";
        public const string Expect = "Expect";
        public const string Expires = "Expires";
        public const string From = "From";
        public const string Host = "Host";
        public const string IfMatch = "If-Match";
        public const string IfModifiedSince = "If-Modified-Since";
        public const string IfNoneMatch = "If-None-Match";
        public const string IfRange = "If-Range";
        public const string IfUnmodifiedSince = "If-Unmodified-Since";
        public const string KeepAlive = "Keep-Alive";
        public const string LastModified = "Last-Modified";
        public const string Location = "Location";
        public const string Origin = "Origin";
        public const string Pragma = "Pragma";
        public const string ProxyConnection = "Proxy-Connection";
        public const string ProxyAuthenticate = "Proxy-Authenticate";
        public const string Range = "Range";
        public const string Referer = "Referer";
        public const string RetryAfter = "Retry-After";
        public const string Server = "Server";
        public const string SetCookie = "Set-Cookie";
        public const string TE = "TE";
        public const string Trailer = "Trailer";
        public const string TransferEncoding = "Transfer-Encoding";
        public const string Upgrade = "Upgrade";
        public const string UserAgent = "User-Agent";
        public const string Warning = "Warning";
        public const string WWWAuthenticate = "WWW-Authenticate";
        public const string Vary = "Vary";
        public const string Via = "Via";

        /// <summary>
        /// A custom response header which indicates that client must accept response from server and don't perform any retry attempts.
        /// </summary>
        public const string DontRetry = "Dont-Retry";

        /// <summary>
        /// A custom response header which indicates that client must reject response from server.
        /// </summary>
        public const string DontAccept = "Dont-Accept";

        /// <summary>
        /// <para>A custom request header which contains client request timeout.</para>
        /// <para>A value specified in seconds with up to 3 decimal digits.</para>
        /// </summary>
        public const string RequestTimeout = "Request-Timeout";

        /// <summary>
        /// <para>A custom request header which contains request priority for throttling and scheduling purposes.</para>
        /// <para>A possible values defined in <see cref="Model.RequestPriority"/> enumeration.</para>
        /// </summary>
        public const string RequestPriority = "Request-Priority";

        /// <summary>
        /// A custom header which contains application name.
        /// </summary>
        public const string ApplicationIdentity = "Application-Identity";

        /// <summary>
        /// A custom optional request header used for transmitting serialized distributed context properties.
        /// </summary>
        public const string ContextProperties = "Context-Properties";

        /// <summary>
        /// A custom optional request header used for transmitting serialized distributed context globals.
        /// </summary>
        public const string ContextGlobals = "Context-Globals";

        /// <summary>
        /// A custom header utilized by <see cref="ForkingRequestStrategy"/> to denote its current parallelism level.
        /// </summary>
        public const string ConcurrencyLevel = "Concurrency-Level";
    }
}