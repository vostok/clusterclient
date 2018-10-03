using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Retry
{
    /// <summary>
    /// Represents a retry policy which uses external predicate to make a decision.
    /// </summary>
    [PublicAPI]
    public class AdHocRetryPolicy : IRetryPolicy
    {
        private readonly Func<Request, RequestParameters, IList<ReplicaResult>, bool> criterion;

        /// <param name="criterion">An external predicate which will be used to make a decision.</param>
        public AdHocRetryPolicy(Func<Request, RequestParameters, IList<ReplicaResult>, bool> criterion)
        {
            this.criterion = criterion;
        }

        /// <inheritdoc />
        public bool NeedToRetry(Request request, RequestParameters parameters, IList<ReplicaResult> results)
            => criterion(request, parameters, results);
    }
}