using System;
using System.Collections.Generic;
using Vostok.ClusterClient.Abstractions;
using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Abstractions.Transforms;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Ordering.Weighed;
using Vostok.ClusterClient.Core.Topology;
using Vostok.ClusterClient.Core.Transforms;

namespace Vostok.ClusterClient.Core
{
    public static class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Initializes configuration's <see cref="IClusterClientConfiguration.ReplicaOrdering"/> with a <see cref="WeighedReplicaOrdering"/> built with a given delegate acting on a <see cref="IWeighedReplicaOrderingBuilder"/> instance.
        /// </summary>
        public static void SetupWeighedReplicaOrdering(this IClusterClientConfiguration configuration, Action<IWeighedReplicaOrderingBuilder> build)
        {
            var builder = new WeighedReplicaOrderingBuilder(configuration.ServiceName, configuration.Log);
            build(builder);
            configuration.ReplicaOrdering = builder.Build();
        }

        /// <summary>
        /// Adds given <paramref name="module"/> to configuration's <see cref="IClusterClientConfiguration.Modules"/> list.
        /// </summary>
        public static void AddRequestModule(this IClusterClientConfiguration configuration, IRequestModule module) =>
            (configuration.Modules ?? (configuration.Modules = new List<IRequestModule>())).Add(module);

        /// <summary>
        /// Adds an <see cref="AdHocResponseTransform"/> with given <paramref name="transform"/> function to configuration's <see cref="IClusterClientConfiguration.ResponseTransforms"/> list.
        /// </summary>
        public static void AddResponseTransform(this IClusterClientConfiguration configuration, Func<Response, Response> transform) =>
            Abstractions.IClusterClientConfigurationExtensions.AddResponseTransform(configuration, new AdHocResponseTransform(transform));

        /// <summary>
        /// Modifies configuration's <see cref="IClusterClientConfiguration.ClusterProvider"/> to repeat all of its replicas <paramref name="repeatCount"/> times.
        /// </summary>
        public static void RepeatReplicas(this IClusterClientConfiguration configuration, int repeatCount)
        {
            if (configuration.ClusterProvider == null)
                return;

            configuration.ClusterProvider = new RepeatingClusterProvider(configuration.ClusterProvider, repeatCount);
        }

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
            
            configuration.AddRequestModule(new AdaptiveThrottlingModule(options));
        }

        /// <summary>
        /// Sets up an adaptive client throttling mechanism with given parameters using <see cref="IClusterClientConfiguration.ServiceName"/> as a storage key.
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
            var options = new AdaptiveThrottlingOptions(configuration.ServiceName, minutesToTrack, minimumRequests, criticalRatio, maximumRejectProbability);
            configuration.AddRequestModule(new AdaptiveThrottlingModule(options));
        }

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
        /// Sets up a replica budgeting mechanism with given parameters using <see cref="IClusterClientConfiguration.ServiceName"/> as a storage key.
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
            var options = new ReplicaBudgetingOptions(configuration.ServiceName, minutesToTrack, minimumRequests, criticalRatio);
            configuration.AddRequestModule(new ReplicaBudgetingModule(options));
        }
    }
}