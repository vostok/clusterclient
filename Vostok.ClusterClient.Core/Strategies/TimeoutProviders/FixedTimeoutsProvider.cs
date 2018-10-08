using System;
using System.Linq;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.Commons.Time;

namespace Vostok.ClusterClient.Core.Strategies.TimeoutProviders
{
    /// <summary>
    /// Represents a timeout provider which issues timeouts from a fixed set of values.
    /// </summary>
    [PublicAPI]
    public class FixedTimeoutsProvider : ISequentialTimeoutsProvider
    {
        private readonly TimeSpan[] timeouts;
        private readonly TailTimeoutBehaviour tailBehaviour;

        /// <param name="tailBehaviour">A behaviour in case when provided timeout values are exhausted.</param>
        /// <param name="timeouts">A timeouts which this provider should return.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timeouts"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="timeouts"/> is empty.</exception>
        public FixedTimeoutsProvider(TailTimeoutBehaviour tailBehaviour, [NotNull] params TimeSpan[] timeouts)
        {
            if (timeouts == null)
                throw new ArgumentNullException(nameof(timeouts));

            if (timeouts.Length == 0)
                throw new ArgumentException("At least one timeout must be specified.", nameof(timeouts));

            this.timeouts = timeouts;
            this.tailBehaviour = tailBehaviour;
        }

        /// <param name="timeouts">A timeouts which this provider should return.</param>
        public FixedTimeoutsProvider([NotNull] params TimeSpan[] timeouts)
            : this(TailTimeoutBehaviour.UseRemainingBudget, timeouts)
        {
        }

        /// <inheritdoc />
        public TimeSpan GetTimeout(Request request, IRequestTimeBudget budget, int currentReplicaIndex, int totalReplicas)
        {
            if (currentReplicaIndex >= timeouts.Length)
                return tailBehaviour == TailTimeoutBehaviour.UseRemainingBudget
                    ? budget.Remaining
                    : TimeSpanArithmetics.Min(timeouts.Last(), budget.Remaining);

            return TimeSpanArithmetics.Min(timeouts[currentReplicaIndex], budget.Remaining);
        }

        /// <inheritdoc />
        public override string ToString() => "fixed";
    }
}