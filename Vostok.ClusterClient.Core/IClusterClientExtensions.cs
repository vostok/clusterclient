using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies;

namespace Vostok.ClusterClient.Core
{
    /// <summary>
    /// Extension methods for IClusterClient interface.
    /// </summary>
    [PublicAPI]
    public static class ClusterClientExtensions
    {
        /// <summary>
        /// <para>Sends given request using given <paramref name="timeout"/>, <paramref name="strategy"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultTimeout"/> if provided <paramref name="timeout"/> is <c>null</c>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if provided <paramref name="strategy"/> is <c>null</c>.</para>
        /// <para>See <see cref="IRequestStrategy.SendAsync"/> for more details about what a request strategy is.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        [PublicAPI, NotNull]
        public static ClusterResult Send(
            [NotNull] this IClusterClient client,
            [NotNull] Request request,
            [CanBeNull] TimeSpan? timeout = null,
            [CanBeNull] IRequestStrategy strategy = null,
            CancellationToken cancellationToken = default,
            [CanBeNull] RequestPriority? priority = null)
        {
            return client.SendAsync(request, timeout, strategy, priority, cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// <para>Sends given request using given <paramref name="timeout"/>, <paramref name="strategy"/>, <paramref name="cancellationToken"/> and <paramref name="priority"/>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultTimeout"/> if provided <paramref name="timeout"/> is <c>null</c>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if provided <paramref name="strategy"/> is <c>null</c>.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultPriority"/> or priority from current ambient context if provided <paramref name="priority"/> is <c>null</c> (explicit > default > context).</para>
        /// <para>See <see cref="IRequestStrategy.SendAsync"/> for more details about what a request strategy is.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        [PublicAPI, ItemNotNull]
        public static Task<ClusterResult> SendAsync(
            [NotNull] this IClusterClient client,
            [NotNull] Request request,
            [CanBeNull] TimeSpan? timeout = null,
            [CanBeNull] IRequestStrategy strategy = null,
            [CanBeNull] RequestPriority? priority = null,
            CancellationToken cancellationToken = default)
            => client.SendAsync(
                request,
                RequestParameters.Empty
                    .WithStrategy(strategy)
                    .WithPriority(priority),
                timeout,
                cancellationToken);
    }
}
