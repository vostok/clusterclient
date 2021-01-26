using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestRetryModule_Tests
    {
        private const int MaxAttempts = 5;

        private int nextModuleCalls;
        private ClusterResult result;
        private IRequestContext context;
        private IRetryPolicy retryPolicy;
        private IRetryStrategy internalDeprecatedRetryStrategy;
        private IRetryStrategyEx retryStrategyEx;
        private RequestRetryModule module;
        private Request request;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("foo/bar");
            result = new ClusterResult(ClusterResultStatus.ReplicasExhausted, new List<ReplicaResult>(), null, request);
            nextModuleCalls = 0;

            context = Substitute.For<IRequestContext>();
            var infinityBudget = Budget.Infinite;
            context.Budget.Returns(infinityBudget);
            context.Log.Returns(new ConsoleLog());
            context.Request.Returns(request);

            retryPolicy = Substitute.For<IRetryPolicy>();
            retryPolicy.NeedToRetry(Arg.Any<Request>(), Arg.Any<RequestParameters>(), Arg.Any<IList<ReplicaResult>>()).Returns(true);

            internalDeprecatedRetryStrategy = Substitute.For<IRetryStrategy>();
            internalDeprecatedRetryStrategy.AttemptsCount.Returns(MaxAttempts);
            internalDeprecatedRetryStrategy.GetRetryDelay(Arg.Any<int>()).Returns(TimeSpan.Zero);
            retryStrategyEx = new RetryStrategyAdapter(internalDeprecatedRetryStrategy);

            module = new RequestRetryModule(retryPolicy, retryStrategyEx);
        }

        [TestCase(ClusterResultStatus.ReplicasExhausted)]
        [TestCase(ClusterResultStatus.ReplicasNotFound)]
        public void Should_use_all_available_attempts_when_retry_is_possible_and_requested(ClusterResultStatus status)
        {
            result = new ClusterResult(status, result.ReplicaResults, result.Response, request);

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(MaxAttempts);
        }

        [Test]
        public void Should_query_retry_possibility_before_each_additional_attempt_including_last_excess_one()
        {
            Execute();

            retryPolicy.Received(5).NeedToRetry(Arg.Any<Request>(), Arg.Any<RequestParameters>(), result.ReplicaResults);
        }

        [Test]
        public void Should_query_retry_delay_before_each_additional_attempt()
        {
            Execute();

            internalDeprecatedRetryStrategy.Received(4).GetRetryDelay(Arg.Any<int>());

            internalDeprecatedRetryStrategy.Received().GetRetryDelay(1);
            internalDeprecatedRetryStrategy.Received().GetRetryDelay(2);
            internalDeprecatedRetryStrategy.Received().GetRetryDelay(3);
            internalDeprecatedRetryStrategy.Received().GetRetryDelay(4);
        }

        [TestCase(ClusterResultStatus.Success)]
        [TestCase(ClusterResultStatus.TimeExpired)]
        [TestCase(ClusterResultStatus.IncorrectArguments)]
        [TestCase(ClusterResultStatus.UnexpectedException)]
        public void Should_not_retry_if_result_has_given_status(ClusterResultStatus status)
        {
            result = new ClusterResult(status, new List<ReplicaResult>(), null, request);

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(1);
        }

        [Test]
        public void Should_not_retry_if_time_budget_has_expired()
        {
            context.Budget.Returns(Budget.Expired);

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(1);
        }

        [Test]
        public void Should_not_retry_if_request_body_stream_has_already_been_used()
        {
            var content = new SingleUseStreamContent(Stream.Null, 100);

            content.Stream.GetHashCode();

            context.Request.Returns(request.WithContent(content));

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(1);
        }

        [Test]
        public void Should_not_retry_if_retry_strategy_attempts_count_is_insufficient()
        {
            var retryStrategy = Substitute.For<IRetryStrategy>();
            retryStrategy.AttemptsCount.Returns(1);
            retryStrategyEx = new RetryStrategyAdapter(retryStrategy);
            module = new RequestRetryModule(retryPolicy, retryStrategyEx);

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(1);
        }

        [Test]
        public void Should_not_retry_if_retry_policy_forbids_it()
        {
            retryPolicy.NeedToRetry(Arg.Any<Request>(), Arg.Any<RequestParameters>(), Arg.Any<IList<ReplicaResult>>()).Returns(false);

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(1);
        }

        [Test]
        public void Should_reset_replica_results_in_native_context_implementation()
        {
            var contextImpl = new RequestContext(
                context.Request,
                parameters: null,
                context.Budget,
                new ConsoleLog(),
                clusterProvider: default,
                replicaOrdering: default,
                transport: default,
                maximumReplicasToUse: int.MaxValue,
                connectionAttempts: default);

            contextImpl.SetReplicaResult(new ReplicaResult(new Uri("http://replica1"), Responses.Timeout, ResponseVerdict.Reject, TimeSpan.Zero));
            contextImpl.SetReplicaResult(new ReplicaResult(new Uri("http://replica2"), Responses.Timeout, ResponseVerdict.Reject, TimeSpan.Zero));

            context = contextImpl;

            Execute();

            contextImpl.FreezeReplicaResults().Should().BeEmpty();
        }

        [Test]
        public void Should_check_cancellation_token_before_each_attempt()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            tokenSource.Cancel();

            context.CancellationToken.Returns(CancellationToken.None, CancellationToken.None, token);

            Action action = () => Execute();

            action.Should().Throw<OperationCanceledException>();

            nextModuleCalls.Should().Be(2);
        }

        [TestCase(ClusterResultStatus.ReplicasExhausted)]
        [TestCase(ClusterResultStatus.ReplicasNotFound)]
        public void Should_call_extended_method_if_Strategy_type_is_IRetryStrategyEx_on_all_available_attempts(ClusterResultStatus status)
        {
            var retryStrategyEx = Substitute.For<IRetryStrategyEx>();
            retryStrategyEx.GetRetryDelay(Arg.Any<IRequestContext>(), Arg.Any<ClusterResult>(), Arg.Any<int>())
                .Returns(
                    info =>
                    {
                        var attempts = (int)info[2];
                        return attempts < MaxAttempts
                            ? (TimeSpan?)TimeSpan.Zero
                            : null;
                    });
            this.retryStrategyEx = retryStrategyEx;
            module = new RequestRetryModule(retryPolicy, retryStrategyEx);

            result = new ClusterResult(status, result.ReplicaResults, result.Response, request);

            Execute().Should().BeSameAs(result);

            nextModuleCalls.Should().Be(MaxAttempts);
            retryStrategyEx.Received(MaxAttempts).GetRetryDelay(Arg.Any<IRequestContext>(), Arg.Any<ClusterResult>(), Arg.Any<int>());
        }

        private ClusterResult Execute()
        {
            return module.ExecuteAsync(
                    context,
                    ctx =>
                    {
                        nextModuleCalls++;
                        return Task.FromResult(result);
                    })
                .GetAwaiter()
                .GetResult();
        }
    }
}