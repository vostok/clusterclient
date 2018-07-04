using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Sending
{
    /// <summary>
    /// A request sending abstraction used by implementations of <see cref="Strategies.IRequestStrategy"/> interface.
    /// </summary>
    public interface IRequestSender
    {
        /// <summary>
        /// <para>Sends given <paramref name="request"/> to given <paramref name="replica"/> with provided <paramref name="timeout"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Returns a <see cref="ReplicaResult"/> with computed <see cref="ResponseVerdict"/> and response time.</para>
        /// </summary>
        [ItemNotNull]
        Task<ReplicaResult> SendToReplicaAsync([NotNull] Uri replica, [NotNull] Request request, TimeSpan timeout, CancellationToken cancellationToken);
    }
}