using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represent the final result of sending request to a cluster of replicas.
    /// </summary>
    // ReSharper disable once InheritdocConsiderUsage
    [PublicAPI]
    public class ClusterResult : IDisposable
    {
        private readonly Response selectedResponse;

        [PublicAPI]
        public ClusterResult(
            ClusterResultStatus status,
            [NotNull] IList<ReplicaResult> replicaResults,
            [CanBeNull] Response selectedResponse,
            [NotNull] Request request)
        {
            this.selectedResponse = selectedResponse;

            Status = status;
            Request = request;
            ReplicaResults = replicaResults;
        }

        /// <summary>
        /// Returns result status. <see cref="ClusterResultStatus.Success"/> value indicates that everything's good.
        /// </summary>
        public ClusterResultStatus Status { get; }

        /// <summary>
        /// <para>Returns the results of replica requests made during request execution.</para>
        /// <para>
        /// Returned list may contain results whose responses have <see cref="ResponseCode.Unknown"/> code.
        /// This usually indicates that request execution has stopped before receiving responses from these replicas.
        /// This can occur when using a <see cref="Strategies.ParallelRequestStrategy"/> or <see cref="Strategies.ForkingRequestStrategy"/>.
        /// </para>
        /// </summary>
        [NotNull]
        public IList<ReplicaResult> ReplicaResults { get; }

        /// <summary>
        /// <para>Returns the final selected response.</para>
        /// <para>By default this property returns a response selected by <see cref="Misc.IResponseSelector"/> implementation.</para>
        /// <para>If no response was received or explicitly selected, this property returns a generated response:</para>
        /// <list type="bullet">
        /// <item><description><see cref="ClusterResultStatus.TimeExpired"/> --> <see cref="ResponseCode.RequestTimeout"/></description></item>
        /// <item><description><see cref="ClusterResultStatus.UnexpectedException"/> --> <see cref="ResponseCode.UnknownFailure"/></description></item>
        /// <item><description>any other status --> <see cref="ResponseCode.Unknown"/></description></item>
        /// </list>
        /// </summary>
        [NotNull]
        public Response Response => selectedResponse ?? GetResponseByStatus();

        /// <summary>
        /// <para>Returns the address of replica which returned final selected <see cref="Response"/>.</para>
        /// <para>May return <c>null</c> if such replica cannot be chosen.</para>
        /// </summary>
        [CanBeNull]
        public Uri Replica => GetFinalReplica();

        /// <summary>
        /// Returns a request which has been sent with this result.
        /// </summary>
        [NotNull]
        public Request Request { get; }

        public void Dispose()
        {
            selectedResponse?.Dispose();

            foreach (var replicaResult in ReplicaResults)
                replicaResult.Response.Dispose();
        }

        private Response GetResponseByStatus()
        {
            switch (Status)
            {
                case ClusterResultStatus.TimeExpired:
                    return Responses.Timeout;

                case ClusterResultStatus.UnexpectedException:
                    return Responses.UnknownFailure;

                case ClusterResultStatus.Canceled:
                    return Responses.Canceled;

                case ClusterResultStatus.Throttled:
                    return Responses.Throttled;

                default:
                    return Responses.Unknown;
            }
        }

        private Uri GetFinalReplica()
        {
            if (ReplicaResults.Count == 0)
                return null;

            var finalResponse = Response;

            var exactlyMatching = ReplicaResults.FirstOrDefault(r => ReferenceEquals(r.Response, finalResponse))?.Replica;
            if (exactlyMatching != null)
                return exactlyMatching;

            var matchingByResponseCode = ReplicaResults.Where(r => r.Response.Code == finalResponse.Code);
            if (matchingByResponseCode.Count() != 1)
                return null;

            return matchingByResponseCode.Single().Replica;
        }
    }
}
