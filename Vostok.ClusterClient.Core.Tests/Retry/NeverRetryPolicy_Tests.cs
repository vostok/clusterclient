using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Retry;

namespace Vostok.Clusterclient.Core.Tests.Retry
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
            policy.NeedToRetry(null, null, new List<ReplicaResult>()).Should().BeFalse();
        }
    }
}