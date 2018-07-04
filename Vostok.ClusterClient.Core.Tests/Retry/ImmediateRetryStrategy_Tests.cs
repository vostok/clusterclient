using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Retry;

namespace Vostok.ClusterClient.Core.Tests.Retry
{
    [TestFixture]
    internal class ImmediateRetryStrategy_Tests
    {
        private ImmediateRetryStrategy strategy;

        [SetUp]
        public void TestSetup()
        {
            strategy = new ImmediateRetryStrategy(5);
        }

        [Test]
        public void Should_return_correct_attempts_count()
        {
            strategy.AttemptsCount.Should().Be(5);
        }

        [Test]
        public void Should_return_zero_retry_delay_for_each_attempt()
        {
            for (var i = 1; i <= 5; i++)
            {
                strategy.GetRetryDelay(i).Should().Be(TimeSpan.Zero);
            }
        }
    }
}
