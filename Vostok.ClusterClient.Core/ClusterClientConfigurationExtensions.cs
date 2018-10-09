using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;

namespace Vostok.Clusterclient.Core
{
    /// <summary>
    /// A set of extensions for IClusterClientConfiguration interface.
    /// </summary>
    [PublicAPI]
    public static class ClusterClientConfigurationExtensions
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
        /// <para>Adds given <paramref name="module"/> to configuration's <see cref="IClusterClientConfiguration.Modules"/> collection.</para>
        /// <para><paramref name="module"/> will be inserted into request module chain once before module of specified type.</para>
        /// </summary>
        /// <param name="type">A type of module before which <paramref name="module"/> will be inserted.</param>
        /// <param name="module">A module to insert into request pipeline.</param>
        /// <param name="configuration">A configuration instance.</param>
        /// <param name="position">A relative position of <paramref name="module"/> from module of type <paramref name="type"/>. This parameter is optional and has default value <see cref="ModulePosition.Before"/>.</param>
        public static void AddRequestModule(
            this IClusterClientConfiguration configuration,
            IRequestModule module,
            Type type,
            ModulePosition position = ModulePosition.Before)
        {
            ObtainModules(configuration, type)[position].Add(module);
        }

        /// <summary>
        /// <para>Adds given <paramref name="module"/> to configuration's <see cref="IClusterClientConfiguration.Modules"/> collection.</para>
        /// <para><paramref name="module"/> will be inserted into request module chain once near <paramref name="relatedModule"/>.</para>
        /// </summary>
        /// <param name="relatedModule">A module near which <paramref name="module"/> will be inserted.</param>
        /// <param name="module">A module to insert into request pipeline.</param>
        /// <param name="configuration">A configuration instance.</param>
        /// <param name="position">A relative position of <paramref name="module"/> from <paramref name="relatedModule"/>. This parameter is optional and has default value <see cref="ModulePosition.Before"/>.</param>
        public static void AddRequestModule(
            this IClusterClientConfiguration configuration,
            IRequestModule module,
            RequestModule relatedModule = RequestModule.Logging,
            ModulePosition position = ModulePosition.Before)
        {
            configuration.AddRequestModule(module, RequestModulesMapping.GetModuleType(relatedModule), position);
        }

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

            configuration.AddRequestModule(new AdaptiveThrottlingModule(options), typeof(AbsoluteUrlSenderModule), ModulePosition.Before);
        }

        /// <summary>
        /// Sets up an adaptive client throttling mechanism with given parameters using <see cref="IClusterClientConfiguration.ServiceName"/> and <see cref="IClusterClientConfiguration.Environment"/> as a storage key.
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
            var options = new AdaptiveThrottlingOptions(GenerateStorageKey(configuration.Environment, configuration.ServiceName), minutesToTrack, minimumRequests, criticalRatio, maximumRejectProbability);
            configuration.AddRequestModule(new AdaptiveThrottlingModule(options), typeof(AbsoluteUrlSenderModule), ModulePosition.Before);

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
        /// Sets up a replica budgeting mechanism with given parameters using <see cref="IClusterClientConfiguration.ServiceName"/> and <see cref="IClusterClientConfiguration.Environment"/> as a storage key.
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
            var options = new ReplicaBudgetingOptions(GenerateStorageKey(configuration.Environment, configuration.ServiceName), minutesToTrack, minimumRequests, criticalRatio);
            configuration.AddRequestModule(new ReplicaBudgetingModule(options), RequestModule.RequestExecution);
        }

        /// <summary>
        /// Enables HTTP request method validation. Valid HTTP methods listed in <see cref="RequestMethods" /> class.
        /// </summary>
        public static void SetupHttpMethodValidation(
            this IClusterClientConfiguration configuration)
        {
            configuration.AddRequestModule(new HttpMethodValidationModule(), typeof(RequestValidationModule), ModulePosition.After);
        }

        /// <summary>
        /// Initializes configuration's <see cref="IClusterClientConfiguration.ResponseCriteria"/> list with given <paramref name="criteria"/>.
        /// </summary>
        public static void SetupResponseCriteria(this IClusterClientConfiguration configuration, params IResponseCriterion[] criteria)
        {
            configuration.ResponseCriteria = new List<IResponseCriterion>(criteria);
        }

        /// <summary>
        /// Adds given <paramref name="transform"/> to configuration's <see cref="IClusterClientConfiguration.RequestTransforms"/> list.
        /// </summary>
        public static void AddRequestTransform(this IClusterClientConfiguration configuration, IRequestTransform transform)
        {
            (configuration.RequestTransforms ?? (configuration.RequestTransforms = new List<IRequestTransform>())).Add(transform);
        }

        /// <summary>
        /// Adds an <see cref="AdHocRequestTransform"/> with given <paramref name="transform"/> function to configuration's <see cref="IClusterClientConfiguration.RequestTransforms"/> list.
        /// </summary>
        public static void AddRequestTransform(this IClusterClientConfiguration configuration, Func<Request, Request> transform)
        {
            AddRequestTransform(configuration, new AdHocRequestTransform(transform));
        }

        /// <summary>
        /// Adds given <paramref name="transform"/> to configuration's <see cref="IClusterClientConfiguration.ResponseTransforms"/> list.
        /// </summary>
        public static void AddResponseTransform(this IClusterClientConfiguration configuration, IResponseTransform transform)
        {
            (configuration.ResponseTransforms ?? (configuration.ResponseTransforms = new List<IResponseTransform>())).Add(transform);
        }

        /// <summary>
        /// Adds an <see cref="AdHocResponseTransform"/> with given <paramref name="transform"/> function to configuration's <see cref="IClusterClientConfiguration.ResponseTransforms"/> list.
        /// </summary>
        public static void AddResponseTransform(this IClusterClientConfiguration configuration, Func<Response, Response> transform) =>
            configuration.AddResponseTransform(new AdHocResponseTransform(transform));

        private static RelatedModules ObtainModules(IClusterClientConfiguration configuration, Type type)
        {
            if (configuration.Modules == null)
                configuration.Modules = new Dictionary<Type, RelatedModules>();

            if (!configuration.Modules.TryGetValue(type, out var modules))
                configuration.Modules[type] = modules = new RelatedModules();
            
            return modules;
        }

        private static string GenerateStorageKey(string environment, string serviceName)
        {
            return string.IsNullOrEmpty(environment)
                ? serviceName
                : (environment, serviceName).ToString();
        }
    }
}