using System;
using System.Collections.Generic;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Retry
{
    /// <summary>
    /// Represents a retry policy which uses external predicate to make a decision.
    /// </summary>
    public class AdHocRetryPolicy : IRetryPolicy
    {
        private readonly Predicate<IList<ReplicaResult>> criterion;

        /// <param name="criterion">An external predicate which will be used to make a decision.</param>
        public AdHocRetryPolicy(Predicate<IList<ReplicaResult>> criterion)
        {
            this.criterion = criterion;
        }

        /// <inheritdoc />
        public bool NeedToRetry(Request request, RequestParameters parameters, IList<ReplicaResult> results) => criterion(results);
    }
}