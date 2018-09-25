using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Strategies.TimeoutProviders
{
    /// <summary>
    /// Represents a timeout provider that combines a <see cref="AdHocTimeoutsProvider"/> for first few requests and uses an <see cref="EqualTimeoutsProvider"/> for the rest of them.
    /// </summary>
    public class AdHocThenEqualTimeoutsProvider : ISequentialTimeoutsProvider
    {
        private readonly AdHocTimeoutsProvider adHocProvider;
        private readonly EqualTimeoutsProvider equalProvider;
        private readonly int fixedTimeoutsCount;

        /// <summary>
        /// Initializes a new instance of <see cref="AdHocThenEqualTimeoutsProvider"/> class.
        /// </summary>
        /// <param name="tailDivisionFactor">A division factor for <see cref="EqualTimeoutsProvider"/>.</param>
        /// <param name="firstTimeouts">An external delegates which will be used to obtain first request timeouts.</param>
        public AdHocThenEqualTimeoutsProvider(int tailDivisionFactor, [NotNull] params Func<TimeSpan>[] firstTimeouts)
        {
            equalProvider = new EqualTimeoutsProvider(tailDivisionFactor);
            adHocProvider = new AdHocTimeoutsProvider(firstTimeouts);
            fixedTimeoutsCount = firstTimeouts.Length;
        }

        /// <inheritdoc />
        public TimeSpan GetTimeout(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas)
        {
            return currentReplicaIndex < fixedTimeoutsCount
                ? adHocProvider.GetTimeout(request, budget, currentReplicaIndex, totalReplicas)
                : equalProvider.GetTimeout(request, budget, currentReplicaIndex - fixedTimeoutsCount, totalReplicas);
        }

        /// <inheritdoc />
        public override string ToString() => $"{adHocProvider} + {equalProvider}";
    }
}