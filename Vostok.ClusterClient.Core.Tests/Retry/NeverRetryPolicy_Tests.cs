using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Retry;

namespace Vostok.ClusterClient.Core.Tests.Retry
{
    [TestFixture]
    internal class NeverRetryPolicy_Tests
    {
        private NeverRetryPolicy policy;

        [SetUp]
        public void TestSetup()
        {
            policy = new NeverRetryPolicy();
        }

        [Test]
        public void NeedToRetry_should_always_return_false()
        {
            policy.NeedToRetry(new List<ReplicaResult>()).Should().BeFalse();
        }
    }
}
