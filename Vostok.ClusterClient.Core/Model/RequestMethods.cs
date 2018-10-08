using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// <para>Contains the names of common HTTP methods.</para>
    /// <para>Values are taken from corresponding RFC (https://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html) with following exceptions:</para>
    /// <list type="bullet">
    /// <item><description><c>CONNECT</c> method is not included.</description></item>
    /// <item><description><c>PATCH</c> method was added as an extension.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public static class RequestMethods
    {
        /// <summary>
        /// GET header name
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// POST header name
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// PUT header name
        /// </summary>
        public const string Put = "PUT";

        /// <summary>
        /// HEAD header name
        /// </summary>
        public const string Head = "HEAD";

        /// <summary>
        /// PATCH header name
        /// </summary>
        public const string Patch = "PATCH";

        /// <summary>
        /// DELETE header name
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// OPTIONS header name
        /// </summary>
        public const string Options = "OPTIONS";

        /// <summary>
        /// TRACE header name
        /// </summary>
        public const string Trace = "TRACE";
    }
}