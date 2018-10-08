using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Retry
{
    /// <summary>
    /// Represents a policy which never chooses to retry cluster communication.
    /// </summary>
    [PublicAPI]
    public class NeverRetryPolicy : IRetryPolicy
    {
        /// <inheritdoc />
        public bool NeedToRetry(Request request, RequestParameters parameters, IList<ReplicaResult> results) => false;
    }
}