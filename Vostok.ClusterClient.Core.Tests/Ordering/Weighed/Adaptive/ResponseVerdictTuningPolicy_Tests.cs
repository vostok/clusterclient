using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive;

namespace Vostok.ClusterClient.Core.Tests.Ordering.Weighed.Adaptive
{
    [TestFixture]
    internal class ResponseVerdictTuningPolicy_Tests
    {
        private ResponseVerdictTuningPolicy policy;

        [SetUp]
        public void TestSetup()
        {
            policy = new ResponseVerdictTuningPolicy();
        }

        [TestCase(ResponseVerdict.Accept, AdaptiveHealthAction.Increase)]
        [TestCase(ResponseVerdict.Reject, AdaptiveHealthAction.Decrease)]
        [TestCase(ResponseVerdict.DontKnow, AdaptiveHealthAction.DontTouch)]
        public void Should_return_correct_action_for_given_response_verdict(ResponseVerdict verdict, AdaptiveHealthAction action)
        {
            var result = new ReplicaResult(new Uri("http://replica"), Responses.Timeout, verdict, TimeSpan.Zero);

            policy.SelectAction(result).Should().Be(action);
        }

        [TestCase(ResponseVerdict.Accept)]
        [TestCase(ResponseVerdict.Reject)]
        [TestCase(ResponseVerdict.DontKnow)]
        public void Should_choose_to_not_touch_health_when_replica_response_code_indicates_stream_reuse_failure(ResponseVerdict verdict)
        {
            var result = new ReplicaResult(new Uri("http://replica"), Responses.StreamReuseFailure, verdict, TimeSpan.Zero);

            policy.SelectAction(result).Should().Be(AdaptiveHealthAction.DontTouch);
        }

        [TestCase(ResponseVerdict.Accept)]
        [TestCase(ResponseVerdict.Reject)]
        [TestCase(ResponseVerdict.DontKnow)]
        public void Should_choose_to_not_touch_health_when_replica_response_code_indicates_stream_input_failure(ResponseVerdict verdict)
        {
            var result = new ReplicaResult(new Uri("http://replica"), Responses.StreamInputFailure, verdict, TimeSpan.Zero);

            policy.SelectAction(result).Should().Be(AdaptiveHealthAction.DontTouch);
        }
    }
}
