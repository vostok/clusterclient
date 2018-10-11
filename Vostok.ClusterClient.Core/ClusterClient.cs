using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core
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
    [PublicAPI]
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
            configuration = new ClusterClientConfiguration(log ?? new SilentLog());

            setup(configuration);

            configuration.ValidateOrDie();
            configuration.AugmentWithDefaults();

            if (configuration.ReplicaTransform != null)
                configuration.ClusterProvider = new TransformingClusterProvider(configuration.ClusterProvider, configuration.ReplicaTransform);

            ReplicaStorageProvider = ReplicaStorageProviderFactory.Create(configuration.ReplicaStorageScope);

            configuration.Transport = new RequestTimeoutTransport(configuration.Transport);
            
            var modules = RequestModuleChainBuilder.BuildChain(configuration, ReplicaStorageProvider);

            pipelineDelegate = RequestModuleChainBuilder.BuildChainDelegate(modules);
        }

        /// <summary>
        /// A <see cref="IClusterProvider"/> implementation that used by this <see cref="ClusterClient"/> instance.
        /// </summary>
        public IClusterProvider ClusterProvider => configuration.ClusterProvider;

        /// <summary>
        /// A <see cref="IReplicaStorageProvider"/> implementation that used by this <see cref="ClusterClient"/> instance.
        /// </summary>
        public IReplicaStorageProvider ReplicaStorageProvider { get; }

        /// <inheritdoc />
        public Task<ClusterResult> SendAsync(
            Request request,
            RequestParameters parameters = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return pipelineDelegate(
                new RequestContext(
                    request,
                    CompleteParameters(parameters),
                    RequestTimeBudget.StartNew(timeout ?? configuration.DefaultTimeout, BudgetPrecision),
                    configuration.Log,
                    configuration.Transport,
                    configuration.MaxReplicasUsedPerRequest,
                    configuration.ClientApplicationName,
                    cancellationToken));
        }

        private RequestParameters CompleteParameters(RequestParameters parameters)
        {
            if (parameters == null)
                return new RequestParameters(configuration.DefaultRequestStrategy, configuration.DefaultPriority);

            if (parameters.Strategy == null)
                parameters = parameters.WithStrategy(configuration.DefaultRequestStrategy);
            if (parameters.Priority == null)
                parameters = parameters.WithPriority(configuration.DefaultPriority);
            if (parameters.ConnectionTimeout == null)
                parameters = parameters.WithConnectionTimeout(configuration.DefaultConnectionTimeout);
            return parameters;
        }
    }
}