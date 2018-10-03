using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Strategies.TimeoutProviders
{
    /// <summary>
    /// Represents a timeout provider that combines a <see cref="FixedTimeoutsProvider"/> for first few requests and uses an <see cref="EqualTimeoutsProvider"/> for the rest of them.
    /// </summary>
    [PublicAPI]
    public class FixedThenEqualTimeoutsProvider : ISequentialTimeoutsProvider
    {
        private readonly FixedTimeoutsProvider fixedProvider;
        private readonly EqualTimeoutsProvider equalProvider;
        private readonly int fixedTimeoutsCount;

        /// <param name="tailDivisionFactor">A division factor for <see cref="EqualTimeoutsProvider"/></param>
        /// <param name="firstTimeouts">A list of timeouts which will be returned for first requests.</param>
        public FixedThenEqualTimeoutsProvider(int tailDivisionFactor, [NotNull] params TimeSpan[] firstTimeouts)
        {
            equalProvider = new EqualTimeoutsProvider(tailDivisionFactor);
            fixedProvider = new FixedTimeoutsProvider(firstTimeouts);
            fixedTimeoutsCount = firstTimeouts.Length;
        }

        /// <inheritdoc />
        public TimeSpan GetTimeout(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas) =>
            currentReplicaIndex < fixedTimeoutsCount
                ? fixedProvider.GetTimeout(request, budget, currentReplicaIndex, totalReplicas)
                : equalProvider.GetTimeout(request, budget, currentReplicaIndex - fixedTimeoutsCount, totalReplicas);

        /// <inheritdoc />
        public override string ToString() => $"{fixedProvider} + {equalProvider}";
    }
}