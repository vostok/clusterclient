using System.Collections.Generic;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Retry
{
    /// <summary>
    /// Represents a policy which never chooses to retry cluster communication.
    /// </summary>
    public class NeverRetryPolicy : IRetryPolicy
    {
        public bool NeedToRetry(IList<ReplicaResult> results) => false;
    }
}