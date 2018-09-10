using System;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive
{
    public sealed class TuningPolicies : IAdaptiveHealthTuningPolicy
    {
        public static readonly IAdaptiveHealthTuningPolicy ByResponseVerdict = new ResponseVerdictTuningPolicy();

        /// <summary>
        /// Creates a <see cref="ResponseTimeTuningPolicy"/> configured by given <paramref name="timeThreshold"/> provider delegate.
        /// </summary>
        public static IAdaptiveHealthTuningPolicy ByResponseTime(Func<TimeSpan> timeThreshold) =>
            new ResponseTimeTuningPolicy(timeThreshold);

        /// <summary>
        /// Creates a <see cref="ResponseTimeTuningPolicy"/> configured by given fixed <paramref name="timeThreshold"/>.
        /// </summary>
        public static IAdaptiveHealthTuningPolicy ByResponseTime(TimeSpan timeThreshold) =>
            ByResponseTime(() => timeThreshold);

        /// <summary>
        /// Creates a <see cref="CompositeTuningPolicy"/> composed of <see cref="ResponseVerdictTuningPolicy"/> and <see cref="ResponseTimeTuningPolicy"/>.
        /// </summary>
        /// <param name="timeThreshold">A response time threshold provider delegate to use for <see cref="ResponseTimeTuningPolicy"/>.</param>
        public static IAdaptiveHealthTuningPolicy ByResponseVerdictAndTime(Func<TimeSpan> timeThreshold) =>
            new CompositeTuningPolicy(ByResponseVerdict, ByResponseTime(timeThreshold));

        /// <summary>
        /// Creates a <see cref="CompositeTuningPolicy"/> composed of <see cref="ResponseVerdictTuningPolicy"/> and <see cref="ResponseTimeTuningPolicy"/>.
        /// </summary>
        /// <param name="timeThreshold">A fixed response time threshold to use for <see cref="ResponseTimeTuningPolicy"/>.</param>
        public static IAdaptiveHealthTuningPolicy ByResponseVerdictAndTime(TimeSpan timeThreshold) =>
            new CompositeTuningPolicy(ByResponseVerdict, ByResponseTime(timeThreshold));

        private TuningPolicies()
        {
        }

        #region Useless implementation

        public AdaptiveHealthAction SelectAction(ReplicaResult result) =>
            throw new NotImplementedException();

        #endregion
    }
}