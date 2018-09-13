using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.ClusterClient.Core.Transport;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.ClusterClient.Core.Topology;
using Vostok.Commons.Collections;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core
{
    /// <summary>
    /// <para>Represents a client used to send HTTP requests to a cluster of replicas.</para>
    /// <para>This implementation guarantees following contracts:</para>
    /// <list type="bullet">
    /// <item><description>It never throws exceptions. All failures are logged and reflected in returned <see cref="ClusterResult"/> instances.</description></item>
    /// <item><description>It is thread-safe. It's recommended to reuse <see cref="ClusterClient"/> instances as much as possible.</description></item>
    /// <item><description>It sends requests with absolute urls directly and does not perform implicit resolving. You can turn them into relative ones with <see cref="IRequestTransform"/>.</description></item>
    /// </list>
    /// <para>A <see cref="ClusterClient"/> instance is constructed by passing an <see cref="ILog"/> and a <see cref="ClusterClientSetup"/> delegate to a constructor.</para>
    /// <para>Provided setup delegate is expected to initialize some fields of an <see cref="IClusterClientConfiguration"/> instance.</para>
    /// <para>The required minimum is to set <see cref="ITransport"/> and <see cref="IClusterProvider"/> implementations.</para>
    /// <example>
    /// <code>
    /// var client = new ClusterClient(log, config =>
    /// {
    ///     config.Transport = new MyTransport();
    ///     config.ClusterProvider = new MyClusterProvider();
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public class ClusterClient : IClusterClient
    {
        private static readonly TimeSpan BudgetPrecision = TimeSpan.FromMilliseconds(15);

        private readonly ClusterClientConfiguration configuration;
        private readonly Func<IRequestContext, Task<ClusterResult>> pipelineDelegate;

        /// <summary>
        /// Creates a <see cref="ClusterClient"/> instance using given <paramref name="log"/> and <paramref name="setup"/> delegate.
        /// </summary>
        /// <exception cref="ClusterClientException">Configuration was incomplete or invalid.</exception>
        public ClusterClient(ILog log, ClusterClientSetup setup)
        {
            configuration = new ClusterClientConfiguration((log ?? new SilentLog()));

            setup(configuration);

            configuration.ValidateOrDie();
            configuration.AugmentWithDefaults();

            if (configuration.ReplicaTransform != null)
                configuration.ClusterProvider = new TransformingClusterProvider(configuration.ClusterProvider, configuration.ReplicaTransform);

            ReplicaStorageProvider = ReplicaStorageProviderFactory.Create(configuration.ReplicaStorageScope);

            var modules = RequestModuleChainBuilder.BuildChain(configuration, ReplicaStorageProvider);

            pipelineDelegate = RequestModuleChainBuilder.BuildChainDelegate(modules);
        }

        public IClusterProvider ClusterProvider => configuration.ClusterProvider;

        public IReplicaStorageProvider ReplicaStorageProvider { get; }

        public Task<ClusterResult> SendAsync(Request request, RequestParameters parameters, TimeSpan? timeout = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return pipelineDelegate(new RequestContext(
                request,
                parameters?.Strategy ?? configuration.DefaultRequestStrategy,
                RequestTimeBudget.StartNew(timeout ?? configuration.DefaultTimeout, BudgetPrecision),
                configuration.Log,
                configuration.Transport,
                parameters?.Priority ?? configuration.DefaultPriority,
                configuration.MaxReplicasUsedPerRequest,
                parameters?.Properties ?? ImmutableArrayDictionary<string, object>.Empty,
                cancellationToken));
        }
    }
}