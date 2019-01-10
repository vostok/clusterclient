using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;

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
        /// <para>Sets up a decorator over current <see cref="ITransport"/> that enriches all requests with given <paramref name="header"/> containing request timeout in seconds in the following format: <c>s.mmm</c>.</para>
        /// </summary>
        public static void SetupRequestTimeoutHeader(this IClusterClientConfiguration configuration, string header = HeaderNames.RequestTimeout)
        {
            if (configuration.Transport == null)
                return;

            configuration.Transport = new TimeoutHeaderTransport(configuration.Transport, header);
        }

        private static string GenerateStorageKey(string environment, string serviceName)
        {
            return (environment ?? string.Empty, serviceName ?? string.Empty).ToString();
        }
    }
}