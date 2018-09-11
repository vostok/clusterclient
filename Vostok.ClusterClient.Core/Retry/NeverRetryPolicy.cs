using System.Collections.Generic;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Retry;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Retry
{
    /// <summary>
    /// Represents a policy which never chooses to retry cluster communication.
    /// </summary>
    public class NeverRetryPolicy : IRetryPolicy
    {
        public bool NeedToRetry(Request request, IList<ReplicaResult> results) => false;
    }
}