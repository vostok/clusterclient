using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core.Sending
{
    internal interface IRequestSenderInternal
    {
        /// <summary>
        /// <para>Sends given <paramref name="request"/> to given <paramref name="replica"/> using given <paramref name="transport"/> with provided <paramref name="connectionTimeout"/>, <paramref name="timeout"/> and <paramref name="cancellationToken"/>.</para>
        /// <para>Returns a <see cref="ReplicaResult"/> with computed <see cref="ResponseVerdict"/> and response time.</para>
        /// </summary>
        [ItemNotNull]
        Task<ReplicaResult> SendToReplicaAsync(
            [NotNull] ITransport transport,
            [NotNull] Uri replica,
            [NotNull] Request request,
            TimeSpan? connectionTimeout,
            TimeSpan timeout,
            CancellationToken cancellationToken);
    }
}