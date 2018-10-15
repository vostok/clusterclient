using System;
using System.Threading;
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
    internal class SingleReplicaRequestStrategy_Tests
    {
        private Uri[] replicas;
        private Request request;
        private RequestParameters parameters;
        private IRequestTimeBudget budget;
        private ReplicaResult result;
        private IRequestSender sender;
        private SingleReplicaRequestStrategy strategy;

        [SetUp]
        public void TestSetup()
        {
            replicas = new[]
            {
                new Uri("http://host1/"),
                new Uri("http://host2/")
            };

            request = Request.Get("foo/bar");
            budget = Budget.WithRemaining(5.Minutes());
            parameters = RequestParameters.Empty.WithConnectionTimeout(1.Seconds());
            result = new ReplicaResult(replicas[0], new Response(ResponseCode.NotFound), ResponseVerdict.Accept, TimeSpan.Zero);

            sender = Substitute.For<IRequestSender>();
            sender.SendToReplicaAsync(null, null, parameters.ConnectionTimeout, TimeSpan.Zero, Arg.Any<CancellationToken>()).ReturnsForAnyArgs(_ => result);

            strategy = new SingleReplicaRequestStrategy();
        }

        [Test]
        public void Should_send_request_to_first_replica_with_correct_parameters()
        {
            var token = new CancellationTokenSource().Token;

            strategy.SendAsync(request, parameters, sender, budget, replicas, replicas.Length, token).Wait();

            sender.Received().SendToReplicaAsync(replicas[0], request, null, budget.Remaining, token);
        }

        [Test]
        public void Should_ignore_connection_timeout()
        {
            parameters = parameters.WithConnectionTimeout(5.Seconds());
            
            var token = new CancellationTokenSource().Token;

            strategy.SendAsync(request, parameters, sender, budget, replicas, replicas.Length, token).Wait();

            sender.Received().SendToReplicaAsync(Arg.Any<Uri>(), Arg.Any<Request>(), null, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void Should_stop_on_first_result_if_its_response_is_accepted()
        {
            strategy.SendAsync(request, parameters, sender, budget, replicas, replicas.Length, CancellationToken.None).Wait();

            sender.ReceivedCalls().Should().HaveCount(1);
        }

        [Test]
        public void Should_stop_on_first_result_if_its_response_is_rejected()
        {
            result = new ReplicaResult(result.Replica, result.Response, ResponseVerdict.Reject, TimeSpan.Zero);

            strategy.SendAsync(request, parameters, sender, budget, replicas, replicas.Length, CancellationToken.None).Wait();

            sender.ReceivedCalls().Should().HaveCount(1);
        }
    }
}