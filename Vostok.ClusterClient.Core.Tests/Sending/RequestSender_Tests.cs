using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Tests.Sending
{
    [TestFixture]
    internal class RequestSender_Tests
    {
        private Uri replica;
        private Request relativeRequest;
        private Request absoluteRequest;
        private Response response;
        private TimeSpan timeout;
        private TimeSpan? connectionTimeout;

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
            connectionTimeout = null;

            configuration = Substitute.For<IClusterClientConfiguration>();
            configuration.ResponseCriteria.Returns(new List<IResponseCriterion> {Substitute.For<IResponseCriterion>()});
            configuration.Logging.Returns(
                new LoggingOptions
                {
                    LogReplicaRequests = true,
                    LogReplicaResults = true
                });
            configuration.ReplicaOrdering.Returns(Substitute.For<IReplicaOrdering>());
            configuration.Log.Returns(log = Substitute.For<ILog>());

            log.IsEnabledFor(default).ReturnsForAnyArgs(true);

            storageProvider = Substitute.For<IReplicaStorageProvider>();

            responseClassifier = Substitute.For<IResponseClassifier>();
            responseClassifier.Decide(Arg.Any<Response>(), Arg.Any<IList<IResponseCriterion>>()).Returns(ResponseVerdict.Accept);

            requestConverter = Substitute.For<IRequestConverter>();
            requestConverter.TryConvertToAbsolute(relativeRequest, replica).Returns(_ => absoluteRequest);

            transport = Substitute.For<ITransport>();
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(_ => response);

            sender = new RequestSender(configuration, storageProvider, responseClassifier, requestConverter);

            configuration.ConnectionAttempts.Returns(1);
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

            transport.Received(1).SendAsync(absoluteRequest, connectionTimeout, Arg.Any<TimeSpan>(), tokenSource.Token);
        }
        
        [Test]
        public void Should_pass_connection_timeout()
        {
            connectionTimeout = 1.Seconds();
            
            Send();

            transport.Received(1).SendAsync(absoluteRequest, connectionTimeout, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_pass_null_connection_timeout_when_it_is_greater_than_full_timeout()
        {
            connectionTimeout = 10.Seconds();
            
            Send();

            transport.Received(1).SendAsync(absoluteRequest, null, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
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
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Throws(new Exception("Fail!"));

            Send().Response.Should().BeSameAs(Responses.UnknownFailure);

            log.Received(1, LogLevel.Error);
        }

        [Test]
        public void Should_not_catch_cancellation_exceptions()
        {
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Throws(new OperationCanceledException());

            Action action = () => Send();

            action.Should().Throw<OperationCanceledException>();
        }

        [Test]
        public void Should_throw_a_cancellation_exception_when_transport_returns_canceled_response()
        {
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).ReturnsTask(Responses.Canceled);

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

            log.Received(2).Log(Arg.Any<LogEvent>());
        }

        [Test]
        public void Should_not_log_requests_and_results_if_not_asked_to()
        {
            configuration.Logging.Returns(
                new LoggingOptions
                {
                    LogReplicaResults = false,
                    LogReplicaRequests = false
                });

            Send();

            log.Received(0).Log(Arg.Any<LogEvent>());
        }

        [Test]
        public void Should_return_stream_reuse_failure_code_upon_catching_according_exception()
        {
            transport
                .SendAsync(null, null, TimeSpan.Zero, CancellationToken.None)
                .ThrowsForAnyArgs(_ => new StreamAlreadyUsedException("No luck here!"));

            Send().Response.Code.Should().Be(ResponseCode.StreamReuseFailure);
        }

        [Test]
        public void Should_retry_connection_timeout()
        {
            configuration.ConnectionAttempts.Returns(2);
            
            transport
                .SendAsync(null, null, TimeSpan.Zero, CancellationToken.None)
                .ReturnsForAnyArgs(new Response(ResponseCode.ConnectFailure), new Response(ResponseCode.Ok));

            Send().Response.Code.Should().Be(ResponseCode.Ok);
            transport.ReceivedWithAnyArgs(2).SendAsync(null, null, TimeSpan.Zero, CancellationToken.None);
        }

        [Test]
        public void Should_respect_connection_attempts_setting()
        {
            configuration.ConnectionAttempts.Returns(1);

            transport
                .SendAsync(null, null, TimeSpan.Zero, CancellationToken.None)
                .ReturnsForAnyArgs(Responses.ConnectFailure, Responses.Ok);

            Send().Response.Code.Should().Be(ResponseCode.ConnectFailure);
            transport.ReceivedWithAnyArgs(1).SendAsync(null, null, TimeSpan.Zero, CancellationToken.None);
        }

        private ReplicaResult Send(CancellationToken token = default)
        {
            return sender.SendToReplicaAsync(transport, replica, relativeRequest, connectionTimeout, timeout, token).GetAwaiter().GetResult();
        }
    }
}