using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class AbsoluteUrlSenderModule_Tests
    {
        private ITransport transport;
        private IResponseClassifier responseClassifier;
        private IList<IResponseCriterion> responseCriteria;
        private IClusterResultStatusSelector resultStatusSelector;

        private IRequestContext context;
        private Request request;
        private Response response;
        private RequestParameters parameters;

        private AbsoluteUrlSenderModule module;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("http://foo/bar");
            response = new Response(ResponseCode.Ok);

            parameters = RequestParameters.Empty.WithConnectionTimeout(1.Seconds());
            
            var budget = Budget.WithRemaining(5.Seconds());

            transport = Substitute.For<ITransport>();
            transport.SendAsync(Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).ReturnsTask(_ => response);

            context = Substitute.For<IRequestContext>();
            context.Request.Returns(_ => request);
            context.Budget.Returns(_ => budget);
            context.Transport.Returns(_ => transport);
            context.Parameters.Returns(_ => parameters);

            responseCriteria = new List<IResponseCriterion>();
            responseClassifier = Substitute.For<IResponseClassifier>();
            responseClassifier.Decide(Arg.Any<Response>(), Arg.Any<IList<IResponseCriterion>>()).Returns(ResponseVerdict.Accept);

            resultStatusSelector = Substitute.For<IClusterResultStatusSelector>();
            resultStatusSelector.Select(null, null).ReturnsForAnyArgs(ClusterResultStatus.Success);

            module = new AbsoluteUrlSenderModule(responseClassifier, responseCriteria, resultStatusSelector);
        }

        [Test]
        public void Should_delegate_to_next_module_when_request_url_is_relative()
        {
            request = Request.Get("foo/bar");

            var result = new ClusterResult(ClusterResultStatus.Success, new List<ReplicaResult>(), response, request);

            Execute(result).Should().BeSameAs(result);

            transport.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_delegate_to_next_module_when_request_url_is_an_absolute_file_url()
        {
            request = Request.Get("/foo/bar/baz");

            var result = new ClusterResult(ClusterResultStatus.Success, new List<ReplicaResult>(), response, request);

            Execute(result).Should().BeSameAs(result);

            transport.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_send_request_using_transport_directly_if_url_is_absolute()
        {
            Execute();

            transport.Received().SendAsync(request, Arg.Any<TimeSpan?>(), 5.Seconds(), context.CancellationToken);
        }

        [Test]
        public void Should_return_canceled_result_if_transport_returns_a_canceled_response()
        {
            response = new Response(ResponseCode.Canceled);

            Execute().Status.Should().Be(ClusterResultStatus.Canceled);
        }

        [Test]
        public void Should_classify_response_to_obtain_a_verdict()
        {
            Execute();

            responseClassifier.Received().Decide(response, responseCriteria);
        }

        [TestCase(ClusterResultStatus.Success)]
        [TestCase(ClusterResultStatus.TimeExpired)]
        [TestCase(ClusterResultStatus.ReplicasExhausted)]
        public void Should_return_result_with_status_given_by_result_status_selector(ClusterResultStatus status)
        {
            resultStatusSelector.Select(null, null).ReturnsForAnyArgs(status);

            Execute().Status.Should().Be(status);
        }

        [Test]
        public void Should_return_result_with_received_response_from_transport()
        {
            Execute().Response.Should().BeSameAs(response);
        }

        [Test]
        public void Should_return_result_with_a_single_correct_replica_result()
        {
            var replicaResult = Execute().ReplicaResults.Should().ContainSingle().Which;

            replicaResult.Replica.Should().BeSameAs(request.Url);
            replicaResult.Response.Should().BeSameAs(response);
            replicaResult.Verdict.Should().Be(ResponseVerdict.Accept);
        }

        [Test]
        public void Should_ignore_connection_timeout()
        {
            parameters = parameters.WithConnectionTimeout(1.Seconds());
            
            Execute();

            transport.Received(1).SendAsync(Arg.Any<Request>(), null, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());

        }

        private ClusterResult Execute(ClusterResult result = null)
        {
            return module.ExecuteAsync(context, _ => Task.FromResult(result)).GetAwaiter().GetResult();
        }
    }
}