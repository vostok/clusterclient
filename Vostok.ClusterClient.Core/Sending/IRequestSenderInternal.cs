using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Sending
{
    internal interface IRequestSenderInternal
    {
        /// <summary>
        /// <para>Sends given <paramref name="request"/> to given <paramref name="replica"/> using given <paramref name="transport"/> with provided <paramref name="timeout"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Returns a <see cref="ReplicaResult"/> with computed <see cref="ResponseVerdict"/> and response time.</para>
        /// </summary>
        [ItemNotNull]
        Task<ReplicaResult> SendToReplicaAsync(
            [NotNull] ITransport transport,
            [NotNull] Uri replica,
            [NotNull] Request request,
            TimeSpan timeout,
            CancellationToken cancellationToken);
    }
}