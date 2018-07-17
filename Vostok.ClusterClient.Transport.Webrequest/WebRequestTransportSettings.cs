using System;
using System.Net;
using Vostok.Commons;
using Vostok.Commons.Conversions;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    public class WebRequestTransportSettings
    {
        public int MaxConnectionsPerEndpoint = 10*1000;
        public bool Pipelined { get; set; } = true;

        public bool FixThreadPoolProblems { get; set; } = true;

        public int ConnectionAttempts { get; set; } = 2;

        public TimeSpan? ConnectionTimeout { get; set; } = 750.Milliseconds();

        public TimeSpan RequestAbortTimeout { get; set; } = 250.Milliseconds();

        public IWebProxy Proxy { get; set; } = null;

        public DataSize? MaxResponseBodySize { get; set; } = null;

        public Predicate<long?> UseResponseStreaming { get; set; } = _ => false;

        public string ConnectionGroupName { get; set; } = null;

        public bool AllowAutoRedirect { get; set; } = false;

        internal Func<int, byte[]> BufferFactory { get; set; } = size => new byte[size];

        internal bool FixNonAsciiHeaders { get; set; } = false;
    }
}