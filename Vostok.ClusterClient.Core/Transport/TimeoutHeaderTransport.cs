using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Transport
{
    /// <summary>
    /// A transport decorator responsible for append <see cref="HeaderNames.RequestTimeout"/> header to request.
    /// </summary>
    internal class TimeoutHeaderTransport : ITransport
    {
        private readonly ITransport transport;
        private readonly string header;

        public TimeoutHeaderTransport(ITransport transport, string header = HeaderNames.RequestTimeout)
        {
            this.transport = transport;
            this.header = header;
        }
        
        public TransportCapabilities Capabilities => transport.Capabilities;

        public Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
            => transport.SendAsync(
                request.WithHeader(header, timeout.TotalSeconds.ToString("0.###s", NumberFormatInfo.InvariantInfo)),
                connectionTimeout,
                timeout,
                cancellationToken);
    }
}