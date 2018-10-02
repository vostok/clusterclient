using System.Collections.Generic;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Retry
{
    /// <summary>
    /// Represents a policy which never chooses to retry cluster communication.
    /// </summary>
    public class NeverRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc />
        public bool NeedToRetry(Request request, RequestParameters parameters, IList<ReplicaResult> results) => false;
    }
}