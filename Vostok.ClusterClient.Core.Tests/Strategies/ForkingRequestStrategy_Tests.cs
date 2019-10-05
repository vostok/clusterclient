using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Sending;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Strategies.DelayProviders;
using Vostok.Clusterclient.Core.Tests.Helpers;

#pragma warning disable CS4014

namespace Vostok.Clusterclient.Core.Tests.Strategies
{
    [TestFixture]
    internal class ForkingRequestStrategy_Tests
    {
        private Uri[] replicas;
        private Request request;
        private List<Request> sentRequests;
        private RequestParameters parameters;
        private IRequestSender sender;
        private IForkingDelaysProvider delaysProvider;
        private IForkingDelaysPlanner delaysPlanner;
        private Dictionary<Uri, TaskCompletionSource<ReplicaResult>> resultSources;
        private List<TaskCompletionSource<bool>> delaySources;
        private IEnumerator<TaskCompletionSource<bool>> delaySourcesEnumerator;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        private ForkingRequestStrategy strategy;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("foo/bar");
            replicas = Enumerable.Range(0, 10).Select(i => new Uri($"http://replica-{i}/")).ToArray();
            resultSources = replicas.ToDictionary(r => r, _ => new TaskCompletionSource<ReplicaResult>());
            parameters = RequestParameters.Empty.WithConnectionTimeout(1.Seconds());
            sentRequests = new List<Request>();

            sender = Substitute.For<IRequestSender>();
            sender
                .SendToReplicaAsync(Arg.Any<Uri>(), Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(info =>
                {
                    sentRequests.Add(info.Arg<Request>());
                    return resultSources[info.Arg<Uri>()].Task;
                });

            delaySources = replicas.Select(_ => new TaskCompletionSource<bool>()).ToList();
            delaySourcesEnumerator = delaySources.GetEnumerator();
            delaysPlanner = Substitute.For<IForkingDelaysPlanner>();
            SetupDelaysPlanner();

            delaysProvider = Substitute.For<IForkingDelaysProvider>();
            SetupForkingDelays(1.Milliseconds());

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            strategy = new ForkingRequestStrategy(delaysProvider, delaysPlanner, 3);
        }

        [Test]
        public void Ctor_should_throw_when_given_negative_parallelism_level()
        {
            Action action = () => new ForkingRequestStrategy(delaysProvider, delaysPlanner, -1);

            action.Should().Throw<ArgumentOutOfRangeException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Ctor_should_throw_when_given_zero_parallelism_level()
        {
            Action action = () => new ForkingRequestStrategy(delaysProvider, delaysPlanner, 0);

            action.Should().Throw<ArgumentOutOfRangeException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Ctor_should_throw_when_given_null_delays_provider()
        {
            Action action = () => new ForkingRequestStrategy(null, 3);

            action.Should().Throw<ArgumentNullException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_immediately_fire_just_one_request()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            sender.ReceivedCalls().Should().HaveCount(1);
        }

        [Test]
        public void Should_fire_initial_request_with_correct_parameters()
        {
            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            sender.Received(1).SendToReplicaAsync(replicas[0], Arg.Any<Request>(), parameters.ConnectionTimeout, 5.Seconds(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_fail_with_bugcheck_exception_if_replicas_enumerable_is_insufficient()
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), new Uri[] {}, replicas.Length, token);

            task.IsFaulted.Should().BeTrue();
            task.Exception.InnerExceptions.Single().Should().BeOfType<InvalidOperationException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_determine_forking_delay_when_firing_a_request()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            delaysProvider.Received(1).GetForkingDelay(request, Budget.Infinite, 0, replicas.Length);
        }

        [Test]
        public void Should_not_try_to_determine_forking_delay_when_parallelism_level_is_already_reached()
        {
            strategy = new ForkingRequestStrategy(delaysProvider, delaysPlanner, 1);

            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            delaysProvider.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_not_try_to_determine_forking_delay_when_there_are_no_more_replicas()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas.Take(1).ToArray(), 1, token);

            delaysProvider.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_invoke_delay_planner_when_produced_forking_delay_is_correct()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            delaysPlanner.Received(1).Plan(1.Milliseconds(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_invoke_delay_planner_when_produced_forking_delay_is_zero()
        {
            SetupForkingDelays(TimeSpan.Zero);

            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            delaysPlanner.Received(1).Plan(TimeSpan.Zero, Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_not_invoke_delay_planner_when_produced_forking_delay_is_negative()
        {
            SetupForkingDelays(-1.Seconds());

            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            delaysPlanner.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_not_invoke_delay_planner_when_produced_forking_delay_exceeds_remaining_time_budget()
        {
            SetupForkingDelays(6.Seconds());

            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            delaysPlanner.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_launch_parallel_requests_when_forking_delay_fires()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteForkingDelay();

            sender.ReceivedCalls().Should().HaveCount(2);

            CompleteForkingDelay();

            sender.ReceivedCalls().Should().HaveCount(3);
        }

        [Test]
        public void Should_launch_parallel_requests_with_correct_parameters()
        {
            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            sender.ClearReceivedCalls();

            CompleteForkingDelay();
            CompleteForkingDelay();

            sender.Received(1).SendToReplicaAsync(replicas[1], Arg.Any<Request>(), parameters.ConnectionTimeout, 5.Seconds(), Arg.Any<CancellationToken>());
            sender.Received(1).SendToReplicaAsync(replicas[2], Arg.Any<Request>(), parameters.ConnectionTimeout, 5.Seconds(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_launch_requests_except_last_with_connection_timeout()
        {
            sender.ClearReceivedCalls();
            
            strategy = new ForkingRequestStrategy(delaysProvider, delaysPlanner, replicas.Length);
            
            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            for (var i = 0; i < replicas.Length; ++i)
                CompleteForkingDelay();

            for (var i = 0; i < replicas.Length - 1; ++i)
                sender.Received(1).SendToReplicaAsync(replicas[i], Arg.Any<Request>(), parameters.ConnectionTimeout, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
            
            sender.Received(1).SendToReplicaAsync(replicas.Last(), Arg.Any<Request>(), null, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Should_stop_when_any_of_requests_completes_with_accepted_result(int replicaIndex)
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            CompleteForkingDelay();
            CompleteForkingDelay();
            CompleteRequest(replicas[replicaIndex], ResponseVerdict.Accept);

            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_issue_another_request_when_a_pending_one_ends_with_rejected_status()
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteForkingDelay();
            CompleteForkingDelay();
            CompleteRequest(replicas[1], ResponseVerdict.Reject);

            task.IsCompleted.Should().BeFalse();

            sender.Received(1).SendToReplicaAsync(replicas[3], Arg.Any<Request>(), parameters.ConnectionTimeout, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_stop_when_all_replicas_ended_up_returning_rejected_statuses()
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteForkingDelay();
            CompleteForkingDelay();

            foreach (var replica in replicas)
            {
                task.IsCompleted.Should().BeFalse();

                CompleteRequest(replica, ResponseVerdict.Reject);
            }

            task.IsCompleted.Should().BeTrue();

            sender.ReceivedCalls().Should().HaveCount(replicas.Length);
            delaysProvider.ReceivedCalls().Should().HaveCount(2);
            delaysPlanner.ReceivedCalls().Should().HaveCount(2);
        }

        [Test]
        public void Should_stop_when_request_stream_gets_used()
        {
            var content = new SingleUseStreamContent(Stream.Null, 100);

            request = Request.Post("foo/bar").WithContent(content);

            var task = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            content.Stream.GetHashCode();

            CompleteForkingDelay();

            task.IsCompleted.Should().BeFalse();

            CompleteRequest(replicas.First(), ResponseVerdict.Reject);

            task.IsCompleted.Should().BeTrue();

            sender.ReceivedCalls().Should().HaveCount(1);
            delaysProvider.ReceivedCalls().Should().HaveCount(1);
            delaysPlanner.ReceivedCalls().Should().HaveCount(1);
        }

        [Test]
        public void Should_forget_existing_forking_delays_upon_any_request_completion()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteForkingDelay();

            CompleteRequest(replicas[0], ResponseVerdict.Reject);

            sender.ClearReceivedCalls();

            CompleteForkingDelay(1);

            sender.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void Should_cancel_remaining_requests_and_delays_when_receiving_accepted_result()
        {
            var tokens = new List<CancellationToken>();

            sender
                .When(s => s.SendToReplicaAsync(Arg.Any<Uri>(), Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()))
                .Do(info => tokens.Add(info.Arg<CancellationToken>()));

            delaysPlanner
                .When(p => p.Plan(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()))
                .Do(info => tokens.Add(info.Arg<CancellationToken>()));

            strategy = new ForkingRequestStrategy(delaysProvider, delaysPlanner, int.MaxValue);

            var sendTask = strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            CompleteForkingDelay();
            CompleteForkingDelay();
            CompleteForkingDelay();

            CompleteRequest(replicas.First(), ResponseVerdict.Accept);

            sendTask.GetAwaiter().GetResult();

            tokens.Should().HaveCount(8);

            foreach (var t in tokens)
            {
                t.IsCancellationRequested.Should().BeTrue();
            }
        }

        [Test]
        public void Should_add_concurrency_level_header_with_value_one_for_sequential_retries()
        {
            var sendTask = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteRequest(replicas[0], ResponseVerdict.Reject);
            CompleteRequest(replicas[1], ResponseVerdict.Reject);
            CompleteRequest(replicas[2], ResponseVerdict.Accept);

            sendTask.GetAwaiter().GetResult();

            sentRequests.Should().HaveCount(3);
            sentRequests[0].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("1");
            sentRequests[1].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("1");
            sentRequests[2].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("1");
        }

        [Test]
        public void Should_add_concurrency_level_header_with_correct_value_for_forked_retries()
        {
            var sendTask = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteForkingDelay();
            CompleteForkingDelay();

            CompleteRequest(replicas[1], ResponseVerdict.Reject);
            CompleteRequest(replicas[2], ResponseVerdict.Reject);
            CompleteRequest(replicas[3], ResponseVerdict.Accept);

            sendTask.GetAwaiter().GetResult();

            sentRequests.Should().HaveCount(5);
            sentRequests[0].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("1");
            sentRequests[1].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("2");
            sentRequests[2].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("3");
            sentRequests[3].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("3");
            sentRequests[4].Headers?[HeaderNames.ConcurrencyLevel].Should().Be("3");
        }

        private void SetupDelaysPlanner()
        {
            delaysPlanner
                .Plan(Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(
                    _ =>
                    {
                        delaySourcesEnumerator.MoveNext();

                        return delaySourcesEnumerator.Current.Task;
                    });
        }

        private void SetupForkingDelays(TimeSpan? first, params TimeSpan?[] next)
        {
            delaysProvider
                .GetForkingDelay(Arg.Any<Request>(), Arg.Any<IRequestTimeBudget>(), Arg.Any<int>(), Arg.Any<int>())
                .Returns(first, next);
        }

        private void CompleteRequest(Uri replica, ResponseVerdict verdict)
        {
            resultSources[replica].TrySetResult(new ReplicaResult(replica, new Response(ResponseCode.Ok), verdict, TimeSpan.Zero));
        }

        private void CompleteForkingDelay()
        {
            delaySourcesEnumerator.Current.TrySetResult(true);
        }

        private void CompleteForkingDelay(int index)
        {
            delaySources[index].TrySetResult(true);
        }
    }
}