using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Sending
{
    internal class ContextualRequestSender : IRequestSender
    {
        private readonly IRequestSenderInternal sender;
        private readonly RequestContext context;

        public ContextualRequestSender(IRequestSenderInternal sender, RequestContext context)
        {
            this.sender = sender;
            this.context = context;
        }

        public async Task<ReplicaResult> SendToReplicaAsync(Uri replica, Request request, TimeSpan timeout, CancellationToken cancellationToken)
        {
            context.SetReplicaResult(CreateUnknownResult(replica));

            try
            {
                var result = await sender.SendToReplicaAsync(context.Transport, replica, request, timeout, cancellationToken).ConfigureAwait(false);
                context.SetReplicaResult(result);
                return result;
            }
            catch (OperationCanceledException)
            {
                context.SetReplicaResult(CreateCanceledResult(replica));
                throw;
            }
        }

        private static ReplicaResult CreateUnknownResult(Uri replica) =>
            new ReplicaResult(replica, Responses.Unknown, ResponseVerdict.DontKnow, TimeSpan.Zero);

        private static ReplicaResult CreateCanceledResult(Uri replica) =>
            new ReplicaResult(replica, Responses.Canceled, ResponseVerdict.DontKnow, TimeSpan.Zero);
    }
}