using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies;

namespace Vostok.ClusterClient.Core.Sending
{
    /// <summary>
    /// A request sending abstraction used by implementations of <see cref="IRequestStrategy"/> interface.
    /// </summary>
    [PublicAPI]
    public interface IRequestSender
    {
        /// <summary>
        /// <para>Sends given <paramref name="request"/> to given <paramref name="replica"/> with provided <paramref name="timeout"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Returns a <see cref="ResponseVerdict"/> with computed <see cref="ReplicaResult"/> and response time.</para>
        /// </summary>
        [ItemNotNull]
        Task<ReplicaResult> SendToReplicaAsync([NotNull] Uri replica, [NotNull] Request request, TimeSpan timeout, CancellationToken cancellationToken);
    }
}
