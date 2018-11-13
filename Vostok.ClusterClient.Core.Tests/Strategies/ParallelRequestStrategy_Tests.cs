using System;
using System.Collections.Generic;
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
using Vostok.Clusterclient.Core.Tests.Helpers;

namespace Vostok.Clusterclient.Core.Tests.Strategies
{
    [TestFixture]
    internal class ParallelRequestStrategy_Tests
    {
        private Uri[] replicas;
        private Request request;
        private RequestParameters parameters;
        private IRequestSender sender;
        private Dictionary<Uri, TaskCompletionSource<ReplicaResult>> resultSources;
        private CancellationTokenSource tokenSource;
        private CancellationToken token;

        private ParallelRequestStrategy strategy;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("foo/bar");
            replicas = Enumerable.Range(0, 10).Select(i => new Uri($"http://replica-{i}/")).ToArray();
            resultSources = replicas.ToDictionary(r => r, _ => new TaskCompletionSource<ReplicaResult>());
            parameters = RequestParameters.Empty.WithConnectionTimeout(1.Seconds());

            sender = Substitute.For<IRequestSender>();
            sender
                .SendToReplicaAsync(Arg.Any<Uri>(), Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(info => resultSources[info.Arg<Uri>()].Task);

            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;

            strategy = new ParallelRequestStrategy(3);
        }

        [Test]
        public void Ctor_should_throw_when_given_negative_parallelism_level()
        {
            Action action = () => new ParallelRequestStrategy(-1);

            action.Should().Throw<ArgumentOutOfRangeException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Ctor_should_throw_when_given_zero_parallelism_level()
        {
            Action action = () => new ParallelRequestStrategy(0);

            action.Should().Throw<ArgumentOutOfRangeException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_immediately_fire_several_requests_to_reach_parallelism_level()
        {
            strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            sender.ReceivedCalls().Should().HaveCount(3);
        }

        [Test]
        public void Should_fire_initial_requests_with_whole_remaining_time_budget()
        {
            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            sender.Received(1).SendToReplicaAsync(replicas[0], request, Arg.Any<TimeSpan?>(), 5.Seconds(), Arg.Any<CancellationToken>());
            sender.Received(1).SendToReplicaAsync(replicas[1], request, Arg.Any<TimeSpan?>(), 5.Seconds(), Arg.Any<CancellationToken>());
            sender.Received(1).SendToReplicaAsync(replicas[2], request, Arg.Any<TimeSpan?>(), 5.Seconds(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_fail_with_bugcheck_exception_if_replicas_enumerable_is_insufficient()
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas.Take(2).ToArray(), replicas.Length, token);

            task.IsFaulted.Should().BeTrue();
            task.Exception.InnerExceptions.Single().Should().BeOfType<InvalidOperationException>().Which.ShouldBePrinted();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Should_stop_when_any_of_the_requests_ends_with_accepted_response(int replicaIndex)
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteRequest(replicas[replicaIndex], ResponseVerdict.Accept);

            task.IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_issue_another_request_when_a_pending_one_ends_with_rejected_status()
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            CompleteRequest(replicas[1], ResponseVerdict.Reject);

            task.IsCompleted.Should().BeFalse();

            sender.Received(1).SendToReplicaAsync(replicas[3], request, parameters.ConnectionTimeout, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_stop_when_all_replicas_ended_up_returning_rejected_statuses()
        {
            var task = strategy.SendAsync(request, parameters, sender, Budget.Infinite, replicas, replicas.Length, token);

            foreach (var replica in replicas)
            {
                CompleteRequest(replica, ResponseVerdict.Reject);
            }

            task.IsCompleted.Should().BeTrue();

            sender.ReceivedCalls().Should().HaveCount(replicas.Length);
        }

        [Test]
        public void Should_fire_initial_requests_to_all_replicas_if_parallelism_level_is_greater_than_replicas_count()
        {
            strategy = new ParallelRequestStrategy(int.MaxValue);

            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            sender.ReceivedCalls().Should().HaveCount(replicas.Length);

            foreach (var replica in replicas)
            {
                sender.Received(1).SendToReplicaAsync(replica, request, Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
            }
        }

        [Test]
        public void Should_ignore_connection_timeout()
        {
            strategy = new ParallelRequestStrategy(int.MaxValue);

            parameters = parameters.WithConnectionTimeout(5.Seconds());

            strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            sender.ReceivedCalls().Should().HaveCount(replicas.Length);

            foreach (var replica in replicas)
            {
                sender.Received(1).SendToReplicaAsync(replica, Arg.Any<Request>(), null, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
            }
        }

        [Test]
        public void Should_cancel_remaining_requests_when_receiving_accepted_result()
        {
            var tokens = new List<CancellationToken>();

            sender
                .When(s => s.SendToReplicaAsync(Arg.Any<Uri>(), Arg.Any<Request>(), Arg.Any<TimeSpan?>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()))
                .Do(info => tokens.Add(info.Arg<CancellationToken>()));

            strategy = new ParallelRequestStrategy(int.MaxValue);

            var sendTask = strategy.SendAsync(request, parameters, sender, Budget.WithRemaining(5.Seconds()), replicas, replicas.Length, token);

            CompleteRequest(replicas.Last(), ResponseVerdict.Accept);

            sendTask.GetAwaiter().GetResult();

            tokens.Should().HaveCount(replicas.Length);

            foreach (var t in tokens)
            {
                t.IsCancellationRequested.Should().BeTrue();
            }
        }

        private void CompleteRequest(Uri replica, ResponseVerdict verdict)
        {
            resultSources[replica].TrySetResult(new ReplicaResult(replica, new Response(ResponseCode.Ok), verdict, TimeSpan.Zero));
        }
    }
}