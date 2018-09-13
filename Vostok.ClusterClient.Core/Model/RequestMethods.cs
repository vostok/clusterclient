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
        public const string Get = "GET";
        public const string Post = "POST";
        public const string Put = "PUT";
        public const string Head = "HEAD";
        public const string Patch = "PATCH";
        public const string Delete = "DELETE";
        public const string Options = "OPTIONS";
        public const string Trace = "TRACE";

        public static readonly HashSet<string> All = new HashSet<string>
        {
            Get, Post, Put, Head, Patch, Delete, Options, Trace
        };
    }
}
