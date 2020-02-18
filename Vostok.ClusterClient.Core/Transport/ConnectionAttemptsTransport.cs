using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Clusterclient.Core.Transport
{
    /// <summary>
    /// A transport decorator responsible for retrying connection failures.
    /// </summary>
    public class ConnectionAttemptsTransport : ITransport
    {
        private readonly ITransport transport;
        private readonly int connectionAttempts;

        public ConnectionAttemptsTransport(
            ITransport transport,
            int connectionAttempts)
        {
            this.transport = transport;
            this.connectionAttempts = connectionAttempts;
        }

        public TransportCapabilities Capabilities => transport.Capabilities;

        public async Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var timeBudget = TimeBudget.StartNew(timeout, TimeSpan.FromMilliseconds(1));

            for (var attempt = 1; attempt <= connectionAttempts; ++attempt)
            {
                var response = await transport.SendAsync(request, connectionTimeout, timeBudget.Remaining, cancellationToken).ConfigureAwait(false);

                if (response.Code == ResponseCode.ConnectFailure)
                    continue;

                return response;
            }

            return new Response(ResponseCode.ConnectFailure);
        }
    }
}