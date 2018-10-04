using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transport
{
    /// <summary>
    /// A transport decorator responsible for append <see cref="HeaderNames.RequestTimeout"/> header to request.
    /// </summary>
    internal class RequestTimeoutTransport : ITransport
    {
        private readonly ITransport transport;

        public RequestTimeoutTransport(ITransport transport) => this.transport = transport;
        public TransportCapabilities Capabilities => transport.Capabilities;

        public Task<Response> SendAsync(Request request, TimeSpan timeout, CancellationToken cancellationToken)
            => transport.SendAsync(
                request.WithHeader(HeaderNames.RequestTimeout, timeout.TotalSeconds.ToString("0.###", NumberFormatInfo.InvariantInfo)),
                timeout,
                cancellationToken);
    }
}