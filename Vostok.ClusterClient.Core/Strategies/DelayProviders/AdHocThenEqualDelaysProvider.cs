using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Strategies.DelayProviders
{
    /// <summary>
    /// Represents a delay provider that combines a <see cref="AdHocDelaysProvider"/> for first few requests and uses an <see cref="EqualDelaysProvider"/> for the rest of them.
    /// </summary>
    public class AdHocThenEqualDelaysProvider : IForkingDelaysProvider
    {
        private readonly AdHocDelaysProvider adHocProvider;
        private readonly EqualDelaysProvider equalProvider;
        private readonly int fixedDelaysCount;

        /// <summary>
        /// Initializes a new instance of <see cref="AdHocThenEqualDelaysProvider"/> class.
        /// </summary>
        public AdHocThenEqualDelaysProvider(int tailDivisionFactor, [NotNull] params Func<TimeSpan>[] firstDelays)
        {
            equalProvider = new EqualDelaysProvider(tailDivisionFactor);
            adHocProvider = new AdHocDelaysProvider(TailDelayBehaviour.StopIssuingDelays, firstDelays);
            fixedDelaysCount = firstDelays.Length;
        }

        /// <inheritdoc />
        public TimeSpan? GetForkingDelay(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas)
        {
            return currentReplicaIndex < fixedDelaysCount
                ? adHocProvider.GetForkingDelay(request, budget, currentReplicaIndex, totalReplicas)
                : equalProvider.GetForkingDelay(request, budget, currentReplicaIndex, totalReplicas);
        }

        /// <inheritdoc />
        public override string ToString() => $"{adHocProvider} + {equalProvider}";
    }
}