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
    /// </summary>
    [PublicAPI]
    public interface IClusterClient
    {
        /// <summary>
        /// <para>Sends given request using given <paramref name="parameters"/>, <paramref name="timeout"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultTimeout"/> if timeout in provided <paramref name="parameters"/> is <c>null</c>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if strategy provided <paramref name="parameters"/> is <c>null</c>.</para>
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
