using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core
{
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Sets up a replica budgeting mechanism with given parameters.
        /// </summary>
        /// <param name="configuration">A configuration to be modified.</param>
        /// <param name="storageKey">See <see cref="ReplicaBudgetingOptions.StorageKey"/>.</param>
        /// <param name="minutesToTrack">See <see cref="ReplicaBudgetingOptions.MinutesToTrack"/>.</param>
        /// <param name="minimumRequests">See <see cref="ReplicaBudgetingOptions.MinimumRequests"/>.</param>
        /// <param name="criticalRatio">See <see cref="ReplicaBudgetingOptions.CriticalRatio"/>.</param>
        public static void SetupReplicaBudgeting(
            this IClusterClientConfiguration configuration,
            string storageKey,
            int minutesToTrack = ClusterClientDefaults.ReplicaBudgetingMinutesToTrack,
            int minimumRequests = ClusterClientDefaults.ReplicaBudgetingMinimumRequests,
            double criticalRatio = ClusterClientDefaults.ReplicaBudgetingCriticalRatio)
        {
            var options = new ReplicaBudgetingOptions(storageKey, minutesToTrack, minimumRequests, criticalRatio);

            configuration.AddRequestModule(new ReplicaBudgetingModule(options));
        }

        /// <summary>
        /// Sets up a replica budgeting mechanism with given parameters using <see cref="IClusterClientConfiguration.TargetServiceName"/> and <see cref="IClusterClientConfiguration.TargetEnvironment"/> as a storage key.
        /// </summary>
        /// <param name="configuration">A configuration to be modified.</param>
        /// <param name="minutesToTrack">See <see cref="ReplicaBudgetingOptions.MinutesToTrack"/>.</param>
        /// <param name="minimumRequests">See <see cref="ReplicaBudgetingOptions.MinimumRequests"/>.</param>
        /// <param name="criticalRatio">See <see cref="ReplicaBudgetingOptions.CriticalRatio"/>.</param>
        public static void SetupReplicaBudgeting(
            this IClusterClientConfiguration configuration,
            int minutesToTrack = ClusterClientDefaults.ReplicaBudgetingMinutesToTrack,
            int minimumRequests = ClusterClientDefaults.ReplicaBudgetingMinimumRequests,
            double criticalRatio = ClusterClientDefaults.ReplicaBudgetingCriticalRatio)
        {
            var storageKey = GenerateStorageKey(configuration.TargetEnvironment, configuration.TargetServiceName);

            SetupReplicaBudgeting(configuration, storageKey, minutesToTrack, minimumRequests, criticalRatio);
        }
    }
}