using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Topology;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.ClusterClient.Core.Transport;
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
    [PublicAPI]
    public interface IClusterClient
    {
        /// <summary>
        /// <para>Sends given request using given <paramref name="parameters"/>, <paramref name="timeout"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultTimeout"/> if timeout in provided <paramref name="parameters"/> is <c>null</c>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultPriority"/> or priority from current ambient context if priority in provided <paramref name="parameters"/> is <c>null</c> (explicit > default > context).</para>
        /// <para>See <see cref="IRequestStrategy.SendAsync"/> for more details about what a request strategy is.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        [ItemNotNull]
        Task<ClusterResult> SendAsync(
            [NotNull] Request request, 
            [CanBeNull] RequestParameters parameters,
            [CanBeNull] TimeSpan? timeout = null, 
            CancellationToken cancellationToken = default);
    }
}
