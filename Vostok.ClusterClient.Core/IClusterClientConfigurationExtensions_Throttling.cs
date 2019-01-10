using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core
{
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Sets up an adaptive client throttling mechanism with given parameters.
        /// </summary>
        /// <param name="configuration">A configuration to be modified.</param>
        /// <param name="storageKey">See <see cref="AdaptiveThrottlingOptions.StorageKey"/>.</param>
        /// <param name="minutesToTrack">See <see cref="AdaptiveThrottlingOptions.MinutesToTrack"/>.</param>
        /// <param name="minimumRequests">See <see cref="AdaptiveThrottlingOptions.MinimumRequests"/>.</param>
        /// <param name="criticalRatio">See <see cref="AdaptiveThrottlingOptions.CriticalRatio"/>.</param>
        /// <param name="maximumRejectProbability">See <see cref="AdaptiveThrottlingOptions.MaximumRejectProbability"/>.</param>
        public static void SetupAdaptiveThrottling(
            this IClusterClientConfiguration configuration,
            string storageKey,
            int minutesToTrack = ClusterClientDefaults.AdaptiveThrottlingMinutesToTrack,
            int minimumRequests = ClusterClientDefaults.AdaptiveThrottlingMinimumRequests,
            double criticalRatio = ClusterClientDefaults.AdaptiveThrottlingCriticalRatio,
            double maximumRejectProbability = ClusterClientDefaults.AdaptiveThrottlingRejectProbabilityCap)
        {
            var options = new AdaptiveThrottlingOptions(
                storageKey,
                minutesToTrack,
                minimumRequests,
                criticalRatio,
                maximumRejectProbability);

            configuration.AddRequestModule(new AdaptiveThrottlingModule(options), typeof(AbsoluteUrlSenderModule));
        }

        /// <summary>
        /// Sets up an adaptive client throttling mechanism with given parameters using <see cref="IClusterClientConfiguration.TargetServiceName"/> and <see cref="IClusterClientConfiguration.TargetEnvironment"/> as a storage key.
        /// </summary>
        /// <param name="configuration">A configuration to be modified.</param>
        /// <param name="minutesToTrack">See <see cref="AdaptiveThrottlingOptions.MinutesToTrack"/>.</param>
        /// <param name="minimumRequests">See <see cref="AdaptiveThrottlingOptions.MinimumRequests"/>.</param>
        /// <param name="criticalRatio">See <see cref="AdaptiveThrottlingOptions.CriticalRatio"/>.</param>
        /// <param name="maximumRejectProbability">See <see cref="AdaptiveThrottlingOptions.MaximumRejectProbability"/>.</param>
        public static void SetupAdaptiveThrottling(
            this IClusterClientConfiguration configuration,
            int minutesToTrack = ClusterClientDefaults.AdaptiveThrottlingMinutesToTrack,
            int minimumRequests = ClusterClientDefaults.AdaptiveThrottlingMinimumRequests,
            double criticalRatio = ClusterClientDefaults.AdaptiveThrottlingCriticalRatio,
            double maximumRejectProbability = ClusterClientDefaults.AdaptiveThrottlingRejectProbabilityCap)
        {
            var storageKey = GenerateStorageKey(configuration.TargetEnvironment, configuration.TargetServiceName);

            SetupAdaptiveThrottling(configuration, storageKey, minutesToTrack, minimumRequests, criticalRatio, maximumRejectProbability);
        }
    }
}