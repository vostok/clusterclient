using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Adaptive
{
    /// <summary>
    /// <para>Represents a tuning policy which selects action based on replica's response time:</para>
    /// <list type="bullet">
    /// <item><description>Response time less than given threshold leads to <see cref="AdaptiveHealthAction.Increase"/> of replica health.</description></item>
    /// <item><description>Response time greater than given threshold leads to <see cref="AdaptiveHealthAction.Decrease"/> of replica health.</description></item>
    /// </list>
    /// </summary>
    [PublicAPI]
    public class ResponseTimeTuningPolicy : IAdaptiveHealthTuningPolicy
    {
        private readonly Func<TimeSpan> thresholdProvider;

        public ResponseTimeTuningPolicy(TimeSpan threshold)
            : this(() => threshold)
        {
        }

        public ResponseTimeTuningPolicy([NotNull] Func<TimeSpan> thresholdProvider)
        {
            this.thresholdProvider = thresholdProvider ?? throw new ArgumentNullException(nameof(thresholdProvider));
        }

        /// <inheritdoc />
        public AdaptiveHealthAction SelectAction(ReplicaResult result) =>
            result.Time >= thresholdProvider() ? AdaptiveHealthAction.Decrease : AdaptiveHealthAction.Increase;
    }
}