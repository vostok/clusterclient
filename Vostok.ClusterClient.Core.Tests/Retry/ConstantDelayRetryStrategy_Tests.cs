using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Retry;

namespace Vostok.Clusterclient.Core.Tests.Retry
{
    [TestFixture]
    internal class ConstantDelayRetryStrategy_Tests
    {
        private ConstantDelayRetryStrategy strategy;

        [SetUp]
        public void TestSetup()
        {
            strategy = new ConstantDelayRetryStrategy(3, 1.Seconds());
        }

        [Test]
        public void Should_return_correct_attempts_count()
        {
            strategy.AttemptsCount.Should().Be(3);
        }

        [Test]
        public void Should_return_same_retry_delay_for_each_attempt()
        {
            strategy.GetRetryDelay(1).Should().Be(1.Seconds());
            strategy.GetRetryDelay(2).Should().Be(1.Seconds());
            strategy.GetRetryDelay(3).Should().Be(1.Seconds());
        }
    }
}