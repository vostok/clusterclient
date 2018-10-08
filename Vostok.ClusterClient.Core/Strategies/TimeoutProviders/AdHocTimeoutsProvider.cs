using System;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.Clusterclient.Core.Strategies.TimeoutProviders
{
    /// <summary>
    /// Represents a timeout provider which issues timeouts using a fixed set of external delegates.
    /// </summary>
    [PublicAPI]
    public class AdHocTimeoutsProvider : ISequentialTimeoutsProvider
    {
        private readonly Func<TimeSpan>[] providers;
        private readonly TailTimeoutBehaviour tailBehaviour;

        /// <param name="tailBehaviour">>A behaviour in case when provided timeout values are exhausted.</param>
        /// <param name="providers">An external delegates which will be used to obtain request timeouts.</param>
        public AdHocTimeoutsProvider(TailTimeoutBehaviour tailBehaviour, [NotNull] params Func<TimeSpan>[] providers)
        {
            if (providers == null)
                throw new ArgumentNullException(nameof(providers));

            if (providers.Length == 0)
                throw new ArgumentException("At least one timeout provider delegate must be specified.", nameof(providers));

            this.providers = providers;
            this.tailBehaviour = tailBehaviour;
        }

        /// <param name="providers">An external delegates which will be used to obtain request timeouts.</param>
        public AdHocTimeoutsProvider([NotNull] params Func<TimeSpan>[] providers)
            : this(TailTimeoutBehaviour.UseRemainingBudget, providers)
        {
        }

        /// <inheritdoc />
        public TimeSpan GetTimeout(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas)
        {
            if (currentReplicaIndex >= providers.Length)
                return tailBehaviour == TailTimeoutBehaviour.UseRemainingBudget
                    ? budget.Remaining
                    : TimeSpanArithmetics.Min(providers.Last()(), budget.Remaining);

            return TimeSpanArithmetics.Min(providers[currentReplicaIndex](), budget.Remaining);
        }

        /// <inheritdoc />
        public override string ToString() => "ad-hoc";
    }
}