using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Weighed.Leadership;

namespace Vostok.ClusterClient.Core.Tests.Ordering.Weighed.Leadership
{
    [TestFixture]
    internal class AcceptedVerdictLeaderDetector_Tests
    {
        private Uri replica;
        private AcceptedVerdictLeaderDetector detector;

        [SetUp]
        public void TestSetup()
        {
            replica = new Uri("http://replica");
            detector = new AcceptedVerdictLeaderDetector();
        }

        [Test]
        public void IsLeaderResult_should_return_true_for_result_with_accept_verdict()
        {
            detector.IsLeaderResult(new ReplicaResult(replica, Responses.Timeout, ResponseVerdict.Accept, TimeSpan.Zero)).Should().BeTrue();
        }

        [Test]
        public void IsLeaderResult_should_return_false_for_result_with_reject_verdict()
        {
            detector.IsLeaderResult(new ReplicaResult(replica, Responses.Timeout, ResponseVerdict.Reject, TimeSpan.Zero)).Should().BeFalse();
        }

        [Test]
        public void IsLeaderResult_should_return_false_for_result_with_dontknow_verdict()
        {
            detector.IsLeaderResult(new ReplicaResult(replica, Responses.Timeout, ResponseVerdict.DontKnow, TimeSpan.Zero)).Should().BeFalse();
        }
    }
}
