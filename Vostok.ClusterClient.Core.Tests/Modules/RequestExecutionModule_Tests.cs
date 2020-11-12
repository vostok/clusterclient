using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestExecutionModule_Tests
    {
        private Uri replica1;
        private Uri replica2;
        private Response response1;
        private Response response2;
        private Response selectedResponse;
        private ReplicaResult result1;
        private ReplicaResult result2;

        private IClusterProvider clusterProvider;
        private List<IReplicasFilter> replicaFilters;
        private IReplicaOrdering replicaOrdering;
        private IResponseSelector responseSelector;
        private IReplicaStorageProvider storageProvider;
        private IRequestSenderInternal requestSender;
        private IClusterResultStatusSelector resultStatusSelector;
        private RequestContext context;
        private RequestExecutionModule module;

        [SetUp]
        public void TestSetup()
        {
            replica1 = new Uri("http://replica1");
            replica2 = new Uri("http://replica2");

            response1 = new Response(ResponseCode.Ok);
            response2 = new Response(ResponseCode.Ok);
            selectedResponse = new Response(ResponseCode.Ok);

            result1 = new ReplicaResult(replica1, response1, ResponseVerdict.DontKnow, TimeSpan.Zero);
            result2 = new ReplicaResult(replica2, response2, ResponseVerdict.DontKnow, TimeSpan.Zero);

            var parameters = RequestParameters.Empty
                .WithStrategy(Substitute.For<IRequestStrategy>());

            clusterProvider = Substitute.For<IClusterProvider>();
            clusterProvider.GetCluster().Returns(new[] {replica1, replica2});

            replicaOrdering = Substitute.For<IReplicaOrdering>();
            replicaOrdering.Order(null, null, null, null).ReturnsForAnyArgs(info => info.Arg<IList<Uri>>().Reverse());

            context = new RequestContext(
                Request.Get("foo/bar"),
                parameters,
                Budget.Infinite,
                new SilentLog(),
                clusterProvider,
                replicaOrdering,
                transport: default,
                maximumReplicasToUse: int.MaxValue,
                connectionAttempts: default);
            context.Parameters.Strategy.SendAsync(null, null, null, null, null, 0, default)
                .ReturnsForAnyArgs(
                    async info =>
                    {
                        var replicas = info.Arg<IEnumerable<Uri>>();
                        var sender = info.Arg<IRequestSender>();

                        foreach (var replica in replicas)
                        {
                            await sender.SendToReplicaAsync(replica, context.Request, null, TimeSpan.Zero, CancellationToken.None);
                        }
                    });

            requestSender = Substitute.For<IRequestSenderInternal>();
            requestSender.SendToReplicaAsync(
                    Arg.Any<ITransport>(),
                    replicaOrdering,
                    replica1,
                    Arg.Any<Request>(),
                    connectionAttempts: Arg.Any<int>(),
                    connectionTimeout: Arg.Any<TimeSpan?>(),
                    timeout: Arg.Any<TimeSpan>(),
                    Arg.Any<CancellationToken>()
                )
                .ReturnsTask(_ => result1);
            requestSender.SendToReplicaAsync(
                    Arg.Any<ITransport>(),
                    replicaOrdering,
                    replica2,
                    Arg.Any<Request>(),
                    connectionAttempts: Arg.Any<int>(),
                    connectionTimeout: Arg.Any<TimeSpan?>(),
                    timeout: Arg.Any<TimeSpan>(),
                    Arg.Any<CancellationToken>()
                )
                .ReturnsTask(_ => result2);

            responseSelector = Substitute.For<IResponseSelector>();
            responseSelector.Select(null, null, null).ReturnsForAnyArgs(_ => selectedResponse);

            resultStatusSelector = Substitute.For<IClusterResultStatusSelector>();
            resultStatusSelector.Select(null, null).ReturnsForAnyArgs(ClusterResultStatus.Success);
            
            replicaFilters = new List<IReplicasFilter>();

            storageProvider = Substitute.For<IReplicaStorageProvider>();
            module = new RequestExecutionModule(
                responseSelector,
                storageProvider,
                requestSender,
                resultStatusSelector,
                replicaFilters);
        }

        [Test]
        public void Should_return_no_replicas_result_when_cluster_provider_returns_null()
        {
            clusterProvider.GetCluster().Returns(null as IList<Uri>);

            Execute().Status.Should().Be(ClusterResultStatus.ReplicasNotFound);
        }

        [Test]
        public void Should_return_no_replicas_result_when_cluster_provider_returns_an_empty_list()
        {
            clusterProvider.GetCluster().Returns(new List<Uri>());

            Execute().Status.Should().Be(ClusterResultStatus.ReplicasNotFound);
        }

        [Test]
        public void Should_return_no_replicas_result_when_single_replica_filter_returns_an_empty_list()
        {
            var mockFilter = Substitute.For<IReplicasFilter>();
            replicaFilters.Add(mockFilter);
            mockFilter.Filter(new List<Uri> {replica1, replica2}, context).Returns(new List<Uri>());

            Execute().Status.Should().Be(ClusterResultStatus.ReplicasNotFound);
        }

        [Test]
        public void Should_return_no_replicas_result_when_second_replica_filter_returns_an_empty_list()
        {
            var mockFilter1 = Substitute.For<IReplicasFilter>();
            var mockFilter2 = Substitute.For<IReplicasFilter>();
            replicaFilters.AddRange(new []{mockFilter1, mockFilter2});
            mockFilter1.Filter(new List<Uri> {replica1, replica2}, context).Returns(new List<Uri>{replica1});
            mockFilter2.Filter(new List<Uri> {replica1}, context).Returns(new List<Uri>());

            Execute().Status.Should().Be(ClusterResultStatus.ReplicasNotFound);
        }

        [Test]
        public void Should_call_second_filter_when_first_replica_filter_returns_an_empty_list()
        {
            var mockFilter1 = Substitute.For<IReplicasFilter>();
            var mockFilter2 = Substitute.For<IReplicasFilter>();
            replicaFilters.AddRange(new []{mockFilter1, mockFilter2});
            mockFilter1.Filter(new List<Uri> {replica1, replica2}, context).Returns(new List<Uri>());

            Execute().Status.Should().Be(ClusterResultStatus.ReplicasNotFound);
            mockFilter2.Received(1).Filter(Arg.Any<IEnumerable<Uri>>(), context);
        }

        [Test]
        public void Should_order_replicas_obtained_from_cluster_provider()
        {
            Execute();

            replicaOrdering.Received().Order(Arg.Is<IList<Uri>>(urls => urls.SequenceEqual(new[] {replica1, replica2})), storageProvider, context.Request, context.Parameters);
        }

        [Test]
        public void Should_invoke_request_strategy_with_correct_parameters()
        {
            Execute();

            context.Parameters.Strategy.Received().SendAsync(context.Request, context.Parameters, Arg.Any<ContextualRequestSender>(), context.Budget, Arg.Is<IEnumerable<Uri>>(urls => urls.SequenceEqual(new[] {replica2, replica1})), 2, context.CancellationToken);
        }

        [Test]
        public void Should_invoke_request_strategy_with_correct_parameters_when_limiting_replicas_count()
        {
            context.MaximumReplicasToUse = 1;

            Execute();

            context.Parameters.Strategy.Received().SendAsync(context.Request, context.Parameters, Arg.Any<ContextualRequestSender>(), context.Budget, Arg.Is<IEnumerable<Uri>>(urls => urls.SequenceEqual(new[] {replica2})), 1, context.CancellationToken);
        }

        [Test]
        public void Should_check_cancellation_token_after_invoking_request_strategy()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.Cancel();

            context = new RequestContext(
                context.Request,
                context.Parameters,
                context.Budget,
                context.Log,
                clusterProvider,
                replicaOrdering,
                transport: default,
                maximumReplicasToUse: int.MaxValue,
                connectionAttempts: default,
                cancellationToken: tokenSource.Token);

            Action action = () => Execute();

            action.Should().Throw<OperationCanceledException>();

            responseSelector.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_select_response_based_on_all_replica_results()
        {
            Execute();

            responseSelector.Received().Select(Arg.Any<Request>(), Arg.Any<RequestParameters>(), Arg.Is<IList<ReplicaResult>>(results => results.SequenceEqual(new[] {result2, result1})));
        }

        [Test]
        public void Should_return_a_result_with_selected_response()
        {
            Execute().Response.Should().BeSameAs(selectedResponse);
        }

        [Test]
        public void Should_return_a_result_with_all_replica_results()
        {
            Execute().ReplicaResults.Should().Equal(result2, result1);
        }

        [TestCase(ClusterResultStatus.Success)]
        [TestCase(ClusterResultStatus.TimeExpired)]
        [TestCase(ClusterResultStatus.ReplicasExhausted)]
        public void Should_return_a_result_with_status_selected_by_result_status_selector(ClusterResultStatus status)
        {
            resultStatusSelector.Select(null, null).ReturnsForAnyArgs(status);

            Execute().Status.Should().Be(status);
        }

        private ClusterResult Execute()
        {
            return module.ExecuteAsync(context, _ => { throw new NotSupportedException(); }).GetAwaiter().GetResult();
        }
    }
}