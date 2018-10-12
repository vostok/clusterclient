using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Sending
{
    internal class RequestSender : IRequestSenderInternal
    {
        private readonly IClusterClientConfiguration configuration;
        private readonly IReplicaStorageProvider storageProvider;
        private readonly IResponseClassifier responseClassifier;
        private readonly IRequestConverter requestConverter;

        public RequestSender(
            IClusterClientConfiguration configuration,
            IReplicaStorageProvider storageProvider,
            IResponseClassifier responseClassifier,
            IRequestConverter requestConverter)
        {
            this.configuration = configuration;
            this.storageProvider = storageProvider;
            this.responseClassifier = responseClassifier;
            this.requestConverter = requestConverter;
        }

        public async Task<ReplicaResult> SendToReplicaAsync(ITransport transport, Uri replica, Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (configuration.Logging.LogReplicaRequests)
                LogRequest(replica, timeout);

            var watch = Stopwatch.StartNew();

            var absoluteRequest = requestConverter.TryConvertToAbsolute(request, replica);

            var response = await SendRequestAsync(transport, absoluteRequest, configuration.ConnectionAttempts, connectionTimeout, timeout, cancellationToken).ConfigureAwait(false);

            var responseVerdict = responseClassifier.Decide(response, configuration.ResponseCriteria);

            var result = new ReplicaResult(replica, response, responseVerdict, watch.Elapsed);

            if (configuration.Logging.LogReplicaResults)
                LogResult(result);

            configuration.ReplicaOrdering.Learn(result, storageProvider);

            return result;
        }

        private async Task<Response> SendRequestAsync(ITransport transport, [CanBeNull] Request request, int connectionAttempts, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (request == null)
                return Responses.Unknown;

            try
            {
                for (var attempt = 1; attempt <= connectionAttempts; ++attempt)
                {
                    var response = await transport.SendAsync(request, connectionTimeout, timeout, cancellationToken).ConfigureAwait(false);
                    
                    if (response.Code == ResponseCode.Canceled)
                        throw new OperationCanceledException();

                    if (response.Code == ResponseCode.ConnectFailure)
                        continue;

                    return response;
                }
                return new Response(ResponseCode.ConnectFailure);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (StreamAlreadyUsedException)
            {
                LogStreamReuseFailure();
                return Responses.StreamReuseFailure;
            }
            catch (Exception error)
            {
                LogTransportException(error);
                return Responses.UnknownFailure;
            }
        }

        #region Logging

        private void LogRequest(Uri replica, TimeSpan timeout) =>
            configuration.Log.Info($"Sending request to replica '{replica}' with timeout {timeout.ToPrettyString()}.");

        private void LogResult(ReplicaResult result) =>
            configuration.Log.Info($"Result: replica = '{result.Replica}'; code = {(int) result.Response.Code} ('{result.Response.Code}'); verdict = {result.Verdict}; time = {result.Time.ToPrettyString()}.");

        private void LogStreamReuseFailure() =>
            configuration.Log.Warn("Detected an attempt to use request body stream more than once, which is not allowed.");

        private void LogTransportException(Exception error) =>
            configuration.Log.Error(error, "Transport implementation threw an exception.");

        #endregion
    }
}