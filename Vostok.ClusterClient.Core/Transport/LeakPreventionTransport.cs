using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Transport;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transport
{
    /// <summary>
    /// A transport decorator responsible for disposing all responses that do not make it to final <see cref="ClusterResult"/>.
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    internal class LeakPreventionTransport : ITransport
    {
        private readonly ITransport transport;
        private readonly object syncObject;

        private List<Response> responses;
        private ClusterResult finalResult;

        public LeakPreventionTransport(ITransport transport)
        {
            this.transport = transport;

            syncObject = new object();
        }

        public TransportCapabilities Capabilities => transport.Capabilities;

        public void CompleteRequest(ClusterResult result)
        {
            lock (syncObject)
            {
                finalResult = result;

                if (responses == null)
                    return;

                foreach (var response in responses)
                    // (iloktionov): Подчистим ресурсы каждого ответа, stream которого не попал в финальный результат для пользователя:
                    if (!EnumerateResponseStreams(finalResult).Any(r => ReferenceEquals(r, response.Stream)))
                        response.Dispose();
            }
        }

        public async Task<Response> SendAsync(Request request, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var response = await transport.SendAsync(request, timeout, cancellationToken).ConfigureAwait(false);

            lock (syncObject)
            {
                if (finalResult == null)
                {
                    if (response.HasStream)
                        (responses ?? (responses = new List<Response>())).Add(response);
                }
                else
                    response.Dispose();
            }

            return response;
        }

        private static IEnumerable<Stream> EnumerateResponseStreams(ClusterResult result)
        {
            var resultResponse = result.Response;

            foreach (var replicaResult in result.ReplicaResults)
            {
                var replicaResponse = replicaResult.Response;

                if (ReferenceEquals(replicaResponse, resultResponse))
                    continue;

                if (replicaResponse.HasStream)
                    yield return replicaResponse.Stream;
            }

            if (resultResponse.HasStream)
                yield return resultResponse.Stream;
        }
    }
}