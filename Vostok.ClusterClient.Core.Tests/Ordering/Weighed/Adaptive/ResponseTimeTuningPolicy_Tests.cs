using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive;

namespace Vostok.ClusterClient.Core.Tests.Ordering.Weighed.Adaptive
{
    [TestFixture]
    internal class ResponseTimeTuningPolicy_Tests
    {
        [Test]
        public void Should_return_decrease_action_if_response_time_is_greater_than_threshold()
        {
            var policy = new ResponseTimeTuningPolicy(5.Seconds());

            policy.SelectAction(CreateResult(6.Seconds())).Should().Be(AdaptiveHealthAction.Decrease);
        }

        [Test]
        public void Should_return_decrease_action_if_response_time_is_equal_to_threshold()
        {
            var policy = new ResponseTimeTuningPolicy(5.Seconds());

            policy.SelectAction(CreateResult(5.Seconds())).Should().Be(AdaptiveHealthAction.Decrease);
        }

        [Test]
        public void Should_return_increase_action_if_response_time_is_less_than_threshold()
        {
            var policy = new ResponseTimeTuningPolicy(5.Seconds());

            policy.SelectAction(CreateResult(4.Seconds())).Should().Be(AdaptiveHealthAction.Increase);
        }

        private ReplicaResult CreateResult(TimeSpan time)
        {
            return new ReplicaResult(new Uri("http://replica"), Responses.Timeout, ResponseVerdict.Accept, time);
        }
    }
}
