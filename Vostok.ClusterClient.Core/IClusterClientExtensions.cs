using System;
using System.Threading;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies;

namespace Vostok.ClusterClient.Core
{
    public static class IClusterClientExtensions
    {
        /// <summary>
        /// <para>Sends given request using given <paramref name="timeout"/>, <paramref name="strategy"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultTimeout"/> if provided <paramref name="timeout"/> is <c>null</c>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if provided <paramref name="strategy"/> is <c>null</c>.</para>
        /// <para>See <see cref="IRequestStrategy.SendAsync"/> for more details about what a request strategy is.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        [NotNull]
        public static ClusterResult Send(
            [NotNull] this IClusterClient client,
            [NotNull] Request request,
            [CanBeNull] TimeSpan? timeout = null,
            [CanBeNull] IRequestStrategy strategy = null,
            CancellationToken cancellationToken = default,
            [CanBeNull] RequestPriority? priority = null)
        {
            return client.SendAsync(request, timeout, strategy, cancellationToken, priority).GetAwaiter().GetResult();
        }
    }
}