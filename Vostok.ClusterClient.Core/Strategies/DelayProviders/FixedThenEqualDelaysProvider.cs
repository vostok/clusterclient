using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Strategies.DelayProviders
{
    /// <summary>
    /// Represents a timeout provider that combines a <see cref="FixedDelaysProvider"/> for first few requests and uses an <see cref="EqualDelaysProvider"/> for the rest of them.
    /// </summary>
    public class FixedThenEqualDelaysProvider : IForkingDelaysProvider
    {
        private readonly FixedDelaysProvider fixedProvider;
        private readonly EqualDelaysProvider equalProvider;
        private readonly int fixedDelaysCount;

        /// <summary>
        /// Initializes a new instance of <see cref="FixedThenEqualDelaysProvider"/> class.
        /// </summary>
        public FixedThenEqualDelaysProvider(int tailDivisionFactor, [NotNull] params TimeSpan[] firstDelays)
        {
            equalProvider = new EqualDelaysProvider(tailDivisionFactor);
            fixedProvider = new FixedDelaysProvider(TailDelayBehaviour.StopIssuingDelays, firstDelays);
            fixedDelaysCount = firstDelays.Length;
        }

        /// <inheritdoc />
        public TimeSpan? GetForkingDelay(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas)
        {
            return currentReplicaIndex < fixedDelaysCount
                ? fixedProvider.GetForkingDelay(request, budget, currentReplicaIndex, totalReplicas)
                : equalProvider.GetForkingDelay(request, budget, currentReplicaIndex, totalReplicas);
        }

        /// <inheritdoc />
        public override string ToString() => $"{fixedProvider} + {equalProvider}";
    }
}