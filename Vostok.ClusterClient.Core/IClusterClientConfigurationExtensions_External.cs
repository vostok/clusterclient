using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;

namespace Vostok.Clusterclient.Core
{
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// <para>Sets up given <paramref name="configuration"/> to send requests to an API behind given external <paramref name="url"/>.</para>
        /// <para>Does not set up a transport.</para>
        /// </summary>
        public static void SetupExternalUrl([NotNull] this IClusterClientConfiguration configuration, [NotNull] Uri url)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (url == null)
                throw new ArgumentNullException(nameof(url));

            if (!url.IsAbsoluteUri)
                throw new ArgumentException($"External url must be an absolute URI. Instead, got this: '{url}'.");

            configuration.ClusterProvider = new FixedClusterProvider(url);

            configuration.MaxReplicasUsedPerRequest = 3;

            configuration.RepeatReplicas(configuration.MaxReplicasUsedPerRequest);

            configuration.ReplicaOrdering = new AsIsReplicaOrdering();

            configuration.SetupReplicaBudgeting(minimumRequests: 10);

            configuration.DefaultRequestStrategy = Strategy.Sequential1;

            configuration.DeduplicateRequestUrl = true;

            configuration.TargetServiceName = url.AbsoluteUri;
        }
    }
}
