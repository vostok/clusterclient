using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class AdaptiveThrottlingModule_Tests
    {
        private const int MinimumRequests = 50;
        private const double CriticalRatio = 2.0;
        private const double ProbabilityCap = 0.8;

        private Uri replica;
        private Request request;
        private ClusterResult acceptedResult;
        private ClusterResult rejectedResult;
        private IRequestContext context;

        private AdaptiveThrottlingOptions options;
        private AdaptiveThrottlingModule module;

        [SetUp]
        public void TestSetup()
        {
            replica = new Uri("http://replica");
            request = Request.Get("foo/bar");
            acceptedResult = new ClusterResult(ClusterResultStatus.Success, new [] { new ReplicaResult(replica, new Response(ResponseCode.Accepted), ResponseVerdict.Accept, TimeSpan.Zero) }, null, request);
            rejectedResult = new ClusterResult(ClusterResultStatus.ReplicasExhausted, new [] { new ReplicaResult(replica, new Response(ResponseCode.TooManyRequests), ResponseVerdict.Reject, TimeSpan.Zero) }, null, request);

            context = Substitute.For<IRequestContext>();
            context.Log.Returns(new SilentLog());

            options = new AdaptiveThrottlingOptions(Guid.NewGuid().ToString(), 1, MinimumRequests, CriticalRatio, ProbabilityCap);
            module = new AdaptiveThrottlingModule(options);
        }

        [Test]
        public void Should_increment_requests_and_accepts_on_accepted_results()
        {
            for (var i = 1; i <= 10; i++)
            {
                Execute(acceptedResult);

                module.Requests.Should().Be(i);
                module.Accepts.Should().Be(i);
            }
        }

        [Test]
        public void Should_increment_only_requests_on_rejected_results()
        {
            for (var i = 1; i <= 10; i++)
            {
                Execute(rejectedResult);

                module.Requests.Should().Be(i);
                module.Accepts.Should().Be(0);
            }
        }

        [Test]
        public void Should_correctly_compute_requests_to_accepts_ratio()
        {
            module.Ratio.Should().Be(0.0);

            Accept(10);

            module.Ratio.Should().Be(1.0);

            Reject(10);

            module.Ratio.Should().Be(2.0);
        }

        [Test]
        public void Should_not_reject_requests_until_minimum_count_is_reached()
        {
            for (var i = 0; i < MinimumRequests - 1; i++)
                Execute(rejectedResult).Should().BeSameAs(rejectedResult);
        }

        [Test]
        public void Should_honor_rejection_probability_cap()
        {
            Accept(1);

            Reject(100);

            module.RejectionProbability.Should().Be(ProbabilityCap);
        }

        [Test]
        public void Should_increase_rejection_probability_as_more_requests_are_rejected()
        {
            Accept(1);
            Reject(1);

            while (module.RejectionProbability < ProbabilityCap)
            {
                var previous = module.RejectionProbability;

                Reject(1);

                module.RejectionProbability.Should().BeGreaterThan(previous);
            }
        }

        [Test]
        public void Should_gradually_decrease_rejection_probability_to_zero_after_requests_become_accepted_after_big_failure()
        {
            Accept(10);

            while (module.RejectionProbability < ProbabilityCap)
            {
                Reject(1);
            }

            for (var i = 0; i < 10 * 1000; i++)
            {
                Accept(1);

                if (module.RejectionProbability <= 0.001)
                    Assert.Pass();
            }

            Assert.Fail("Rejection probability did not vanish after 10k accepts.");
        }

        [Test]
        public void Should_reject_with_throttled_result_when_rejection_probability_allows()
        {
            options = new AdaptiveThrottlingOptions(Guid.NewGuid().ToString(), 1, MinimumRequests, CriticalRatio, 1.0);
            module = new AdaptiveThrottlingModule(options);

            Accept(1);

            while (module.RejectionProbability < 0.999)
                Reject(1);

            for (var i = 0; i < 100; i++)
            {
                var requestsBefore = module.Requests;
                var acceptsBefore = module.Accepts;

                var result = Execute(acceptedResult);
                if (result.Status == ClusterResultStatus.Throttled)
                {
                    module.Requests.Should().Be(requestsBefore + 1);
                    module.Accepts.Should().Be(acceptsBefore);
                    Assert.Pass();
                }
            }

            Assert.Fail("No requests were rejected in 100 attempts, which was highly expected.");
        }

        [Test]
        [Explicit]
        public void Should_forget_old_information_as_time_passes()
        {
            Accept(100);
            Reject(100);

            Thread.Sleep(1.Minutes() + 5.Seconds());

            module.Requests.Should().Be(0);
            module.Accepts.Should().Be(0);
        }

        [Test]
        public void Should_not_account_for_requests_still_in_progress()
        {
            var tcs = new TaskCompletionSource<ClusterResult>();

            var tasks = new List<Task<ClusterResult>>();

            for (var i = 0; i < 500; i++)
            {
                tasks.Add(module.ExecuteAsync(context, _ => tcs.Task));
            }

            module.Requests.Should().Be(0);
            module.Accepts.Should().Be(0);

            tcs.TrySetResult(rejectedResult);

            foreach (var task in tasks)
            {
                task.GetAwaiter().GetResult().Should().BeSameAs(rejectedResult);
            }

            module.Requests.Should().Be(500);
            module.Accepts.Should().Be(0);

            Console.Out.WriteLine(module.RejectionProbability);
        }

        private void Accept(int count)
        {
            for (var i = 0; i < count; i++)
                Execute(acceptedResult);
        }

        private void Reject(int count)
        {
            for (var i = 0; i < count; i++)
                Execute(rejectedResult);
        }

        private ClusterResult Execute(ClusterResult result)
        {
            return module.ExecuteAsync(context, _ => Task.FromResult(result)).GetAwaiter().GetResult();
        }
    }
}
