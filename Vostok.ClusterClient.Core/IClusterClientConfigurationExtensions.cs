using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Clusterclient.Core
{
    /// <summary>
    /// A set of extensions for IClusterClientConfiguration interface.
    /// </summary>
    [PublicAPI]
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// Initializes configuration's <see cref="IClusterClientConfiguration.ReplicaOrdering"/> with a <see cref="WeighedReplicaOrdering"/> built with a given delegate acting on a <see cref="IWeighedReplicaOrderingBuilder"/> instance.
        /// </summary>
        public static void SetupWeighedReplicaOrdering(this IClusterClientConfiguration configuration, Action<IWeighedReplicaOrderingBuilder> build)
        {
            var builder = new WeighedReplicaOrderingBuilder(configuration.TargetEnvironment, configuration.TargetServiceName, configuration.Log);
            build(builder);
            configuration.ReplicaOrdering = builder.Build();
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
        /// Enables HTTP request method validation. Valid HTTP methods listed in <see cref="RequestMethods" /> class.
        /// </summary>
        public static void SetupHttpMethodValidation(this IClusterClientConfiguration configuration)
        {
            configuration.AddRequestModule(new HttpMethodValidationModule(), typeof(RequestValidationModule), ModulePosition.After);
        }

        /// <summary>
        /// Enables fixing of undertuned <see cref="System.Threading.ThreadPool"/> limits upon encountering request timeouts.
        /// </summary>
        public static void SetupThreadPoolLimitsTuning(this IClusterClientConfiguration configuration)
        {
            configuration.AddRequestModule(ThreadPoolTuningModule.Instance, typeof(LoggingModule));
        }

        /// <summary>
        /// Initializes configuration's <see cref="IClusterClientConfiguration.ResponseCriteria"/> list with given <paramref name="criteria"/>.
        /// </summary>
        public static void SetupResponseCriteria(this IClusterClientConfiguration configuration, params IResponseCriterion[] criteria)
        {
            configuration.ResponseCriteria = new List<IResponseCriterion>(criteria);
        }
        
        /// <summary>
        /// Adds given <paramref name="filter"/> to configuration's <see cref="IClusterClientConfiguration.ReplicasFilters"/> list.
        /// </summary>
        public static void AddReplicasFilter(this IClusterClientConfiguration configuration, IReplicasFilter filter)
        {
            (configuration.ReplicasFilters ?? (configuration.ReplicasFilters = new List<IReplicasFilter>())).Add(filter);
        }

        private static string GenerateStorageKey(string environment, string serviceName)
        {
            return (environment ?? string.Empty, serviceName ?? string.Empty).ToString();
        }
    }
}