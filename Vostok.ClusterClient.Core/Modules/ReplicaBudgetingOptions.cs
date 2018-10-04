using System;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// <para>Represents a configuration of <see cref="ReplicaBudgetingModule"/> instance.</para>
    /// </summary>
    [PublicAPI]
    public class ReplicaBudgetingOptions
    {
        /// <param name="storageKey">A key used to decouple statistics for different services.</param>
        /// <param name="minutesToTrack">How much minutes of statistics will be tracked. Should be >= 1.</param>
        /// <param name="minimumRequests">A minimum requests count in <see cref="MinutesToTrack"/> minutes to limit available replicas for request.</param>
        /// <param name="criticalRatio">A maximum allowed ratio of used replicas count to issued requests count. Should be in (1; +inf) range.</param>
        /// <exception cref="ArgumentNullException"><paramref name="storageKey"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minutesToTrack"/> less than 1 or <paramref name="criticalRatio"/> not in (1; +inf) range.</exception>
        public ReplicaBudgetingOptions(
            [NotNull] string storageKey,
            int minutesToTrack = ClusterClientDefaults.ReplicaBudgetingMinutesToTrack,
            int minimumRequests = ClusterClientDefaults.ReplicaBudgetingMinimumRequests,
            double criticalRatio = ClusterClientDefaults.ReplicaBudgetingCriticalRatio)
        {
            if (storageKey == null)
                throw new ArgumentNullException(nameof(storageKey));

            if (minutesToTrack < 1)
                throw new ArgumentOutOfRangeException(nameof(minutesToTrack), "Minutes to track parameter must be >= 1.");

            if (criticalRatio <= 1.0)
                throw new ArgumentOutOfRangeException(nameof(criticalRatio), "Critical ratio must be in (1; +inf) range.");

            StorageKey = storageKey;
            MinutesToTrack = minutesToTrack;
            MinimumRequests = minimumRequests;
            CriticalRatio = criticalRatio;
        }

        /// <summary>
        /// A key used to decouple statistics for different services.
        /// </summary>
        [NotNull]
        public string StorageKey { get; }

        /// <summary>
        /// How much minutes of statistics will be tracked. Must be >= 1.
        /// </summary>
        public int MinutesToTrack { get; }

        /// <summary>
        /// A minimum requests count in <see cref="MinutesToTrack"/> minutes to limit available replicas for request. Must lie in (1; +inf) range.
        /// </summary>
        public int MinimumRequests { get; }

        /// <summary>
        /// A maximum allowed ratio of used replicas count to issued requests count.
        /// </summary>
        public double CriticalRatio { get; }
    }
}