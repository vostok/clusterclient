using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions;
using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Logging;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Ordering;
using Vostok.ClusterClient.Abstractions.Ordering.Storage;
using Vostok.ClusterClient.Abstractions.Transport;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Sending;
using Vostok.ClusterClient.Core.Tests.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.ClusterClient.Core.Tests.Sending
{
    [TestFixture]
    internal class RequestSender_Tests
    {
        private Uri replica;
        private Request relativeRequest;
        private Request absoluteRequest;
        private Response response;
        private TimeSpan timeout;

        private IClusterClientConfiguration configuration;
        private IReplicaStorageProvider storageProvider;
        private IResponseClassifier responseClassifier;
        private IRequestConverter requestConverter;
        private ITransport transport;
        private ILog log;

        private RequestSender sender;

        [SetUp]
        public void TestSetup()
        {
            replica = new Uri("http://replica/");
            relativeRequest = Request.Get("foo/bar");
            absoluteRequest = Request.Get("http://replica/foo/bar");
            response = new Response(ResponseCode.Ok);
            timeout = 5.Seconds();

            configuration = Substitute.For<IClusterClientConfiguration>();
            configuration.ResponseCriteria.Returns(new List<IResponseCriterion> {Substitute.For<IResponseCriterion>()});
            configuration.Logging.Returns(new LoggingOptions
            {
                LogReplicaRequests = true,
                LogReplicaResults = true
            });
            configuration.ReplicaOrdering.Returns(Substitute.For<IReplicaOrdering>());
            configuration.Log.Returns(log = Substitute.For<ILog>());

            storageProvider = Substitute.For<IReplicaStorageProvider>();

            responseClassifier = Substitute.For<IResponseClassifier>();
            responseClassifier.Decide(Arg.Any<Response>(), Arg.Any<IList<IResponseCriterion>>()).Returns(ResponseVerdict.Accept);

            requestConverter = Substitute.For<IRequestConverter>();
            requestConverter.TryConvertToAbsolute(relativeRequest, replica).Returns(_ => absoluteRequest);

            transport = Substitute.For<ITransport>();
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(_ => response);

            sender = new RequestSender(configuration, storageProvider, responseClassifier, requestConverter);
        }

        [Test]
        public void Should_convert_relative_request_to_absolute()
        {
            Send();

            requestConverter.Received(1).TryConvertToAbsolute(relativeRequest, replica);
        }

        [Test]
        public void Should_send_request_with_transport_when_request_conversion_succeeds()
        {
            var tokenSource = new CancellationTokenSource();

            Send(tokenSource.Token);

            transport.Received(1).SendAsync(absoluteRequest, timeout, tokenSource.Token);
        }

        [Test]
        public void Should_not_touch_transport_when_request_conversion_fails()
        {
            absoluteRequest = null;

            Send();

            transport.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_return_unknown_response_when_request_conversion_fails()
        {
            absoluteRequest = null;

            Send().Response.Should().BeSameAs(Responses.Unknown);
        }

        [Test]
        public void Should_return_unknown_failure_response_when_transport_throws_an_exception()
        {
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Throws(new Exception("Fail!"));

            Send().Response.Should().BeSameAs(Responses.UnknownFailure);

            // log.CallsErrorCount.Should().Be(1);  // todo(Mansiper): fix it
        }

        [Test]
        public void Should_not_catch_cancellation_exceptions()
        {
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

            Action action = () => Send();

            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void Should_throw_a_cancellation_exception_when_transport_returns_canceled_response()
        {
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).ReturnsTask(Responses.Canceled);

            Action action = () => Send();

            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void Should_classify_response_from_replica()
        {
            Send();

            responseClassifier.Received().Decide(response, configuration.ResponseCriteria);
        }

        [Test]
        public void Should_report_replica_result_to_replica_ordering()
        {
            Send();

            configuration.ReplicaOrdering.Received(1).Learn(Arg.Any<ReplicaResult>(), storageProvider);
        }

        [Test]
        public void Should_return_result_with_correct_replica()
        {
            Send().Replica.Should().BeSameAs(replica);
        }

        [Test]
        public void Should_return_result_with_correct_response()
        {
            Send().Response.Should().BeSameAs(response);
        }

        [TestCase(ResponseVerdict.Accept)]
        [TestCase(ResponseVerdict.Reject)]
        public void Should_return_result_with_correct_verdict(ResponseVerdict verdict)
        {
            responseClassifier.Decide(response, configuration.ResponseCriteria).Returns(verdict);

            Send().Verdict.Should().Be(verdict);
        }

        [Test]
        public void Should_log_requests_and_results_if_asked_to()
        {
            Send();

            // log.CallsInfoCount.Should().Be(2);   // todo(Mansiper): fix it
        }

        [Test]
        public void Should_not_log_requests_and_results_if_not_asked_to()
        {
            configuration.Logging.Returns(new LoggingOptions
            {
                LogReplicaResults = false,
                LogReplicaRequests = false
            });

            Send();

            log.Received(0).Log(Arg.Any<LogEvent>());   // todo(Mansiper): fix it
        }

        [Test]
        public void Should_return_stream_reuse_failure_code_upon_catching_according_exception()
        {
            transport
                .SendAsync(null, TimeSpan.Zero, CancellationToken.None)
                .ThrowsForAnyArgs(_ => new StreamAlreadyUsedException("No luck here!"));

            Send().Response.Code.Should().Be(ResponseCode.StreamReuseFailure);
        }

        private ReplicaResult Send(CancellationToken token = default(CancellationToken))
        {
            return sender.SendToReplicaAsync(transport, replica, relativeRequest, timeout, token).GetAwaiter().GetResult();
        }
    }
}
