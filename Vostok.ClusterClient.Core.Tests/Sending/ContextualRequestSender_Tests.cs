using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Core.Tests.Sending
{
    [TestFixture]
    internal class ContextualRequestSender_Tests
    {
        private Uri replica;
        private Request request;
        private ReplicaResult result;
        private TimeSpan timeout;

        private TaskCompletionSource<ReplicaResult> resultSource;
        private IRequestSenderInternal baseSender;
        private RequestContext context;
        private ContextualRequestSender contextualSender;

        [SetUp]
        public void TestSetup()
        {
            replica = new Uri("http://replica");
            request = Request.Get("foo/bar");
            result = new ReplicaResult(replica, new Response(ResponseCode.Ok), ResponseVerdict.Accept, 1.Milliseconds());
            timeout = 1.Minutes();

            resultSource = new TaskCompletionSource<ReplicaResult>();

            baseSender = Substitute.For<IRequestSenderInternal>();
            baseSender.SendToReplicaAsync(null, null, null, TimeSpan.Zero, CancellationToken.None).ReturnsForAnyArgs(_ => resultSource.Task);

            context = new RequestContext(request, new RequestParameters(Strategy.SingleReplica), Budget.WithRemaining(timeout), new ConsoleLog(), Substitute.For<ITransport>(), int.MaxValue, null, CancellationToken.None);
            contextualSender = new ContextualRequestSender(baseSender, context);
        }

        [Test]
        public void Should_add_default_replica_result_to_context_before_sending_request()
        {
            var sendTask = contextualSender.SendToReplicaAsync(replica, request, timeout, CancellationToken.None);

            var defaultResult = context.FreezeReplicaResults().Should().ContainSingle().Which;

            defaultResult.Replica.Should().BeSameAs(replica);
            defaultResult.Response.Should().BeSameAs(Responses.Unknown);
            defaultResult.Verdict.Should().Be(ResponseVerdict.DontKnow);

            CompleteSending();

            sendTask.GetAwaiter().GetResult();
        }

        [Test]
        public void Should_add_real_replica_result_to_context_after_sending_request()
        {
            var sendTask = contextualSender.SendToReplicaAsync(replica, request, timeout, CancellationToken.None);

            CompleteSending();

            sendTask.GetAwaiter().GetResult();

            context.FreezeReplicaResults().Should().ContainSingle().Which.Should().BeSameAs(result);
        }

        [Test]
        public void Should_return_result_from_base_request_sender()
        {
            var sendTask = contextualSender.SendToReplicaAsync(replica, request, timeout, CancellationToken.None);

            CompleteSending();

            sendTask.GetAwaiter().GetResult().Should().BeSameAs(result);
        }

        [Test]
        public void Should_pass_cancellation_token_to_base_request_sender()
        {
            var tokenSource = new CancellationTokenSource();

            CompleteSending();

            contextualSender.SendToReplicaAsync(replica, request, timeout, tokenSource.Token).GetAwaiter().GetResult();

            baseSender.Received().SendToReplicaAsync(context.Transport, replica, request, timeout, tokenSource.Token);
        }

        [Test]
        public void Should_pass_cancellation_exceptions_through()
        {
            Task.Run(() => resultSource.TrySetException(new OperationCanceledException()));

            Action action = () => contextualSender.SendToReplicaAsync(replica, request, timeout, CancellationToken.None).GetAwaiter().GetResult();

            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void Should_set_canceled_result_for_replica_upon_seeing_cancellation_exception()
        {
            Task.Run(() => resultSource.TrySetException(new OperationCanceledException()));

            try
            {
                contextualSender.SendToReplicaAsync(replica, request, timeout, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
            }

            var replicaResult = context.FreezeReplicaResults().Should().ContainSingle().Which;

            replicaResult.Replica.Should().BeSameAs(replica);
            replicaResult.Response.Should().BeSameAs(Responses.Canceled);
            replicaResult.Verdict.Should().Be(ResponseVerdict.DontKnow);
        }

        private void CompleteSending()
        {
            Task.Run(() => resultSource.TrySetResult(result));
        }
    }
}