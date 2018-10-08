using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Strategies.DelayProviders
{
    /// <summary>
    /// Represents a delay provider that combines a <see cref="AdHocDelaysProvider"/> for first few requests and uses an <see cref="EqualDelaysProvider"/> for the rest of them.
    /// </summary>
    [PublicAPI]
    public class AdHocThenEqualDelaysProvider : IForkingDelaysProvider
    {
        private readonly AdHocDelaysProvider adHocProvider;
        private readonly EqualDelaysProvider equalProvider;
        private readonly int fixedDelaysCount;

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