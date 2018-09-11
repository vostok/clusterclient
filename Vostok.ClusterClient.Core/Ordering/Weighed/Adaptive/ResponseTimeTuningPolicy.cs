using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive
{
    /// <summary>
    /// <para>Represents a tuning policy which selects action based on replica's response time:</para>
    /// <list type="bullet">
    /// <item>Response time less than given threshold leads to <see cref="AdaptiveHealthAction.Increase"/> of replica health.</item>
    /// <item>Response time greater than given threshold leads to <see cref="AdaptiveHealthAction.Decrease"/> of replica health.</item>
    /// </list>
    /// </summary>
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

        public AdaptiveHealthAction SelectAction(ReplicaResult result) =>
            result.Time >= thresholdProvider() ? AdaptiveHealthAction.Decrease : AdaptiveHealthAction.Increase;
    }
}