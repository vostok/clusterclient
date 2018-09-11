using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies.TimeoutProviders;
using Vostok.ClusterClient.Core.Tests.Helpers;

namespace Vostok.ClusterClient.Core.Tests.Strategies.TimeoutProviders
{
    [TestFixture]
    internal class FixedTimeoutsProvider_Tests
    {
        private Request request;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("/foo");
        }

        [Test]
        public void Should_throw_an_error_when_given_null_timeouts_array()
        {
            Action action = () => new FixedTimeoutsProvider(null);

            action.Should().Throw<ArgumentNullException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_throw_an_error_when_given_an_empty_timeouts_array()
        {
            Action action = () => new FixedTimeoutsProvider();

            action.Should().Throw<ArgumentException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_return_all_fixed_timeouts_one_by_one()
        {
            var provider = new FixedTimeoutsProvider(5.Seconds(), 3.Seconds(), 10.Seconds());

            provider.GetTimeout(request, Budget.Infinite, 0, 5).Should().Be(5.Seconds());
            provider.GetTimeout(request, Budget.Infinite, 1, 5).Should().Be(3.Seconds());
            provider.GetTimeout(request, Budget.Infinite, 2, 5).Should().Be(10.Seconds());
        }

        [Test]
        public void Should_limit_fixed_timeouts_by_remaining_time_budget()
        {
            var provider = new FixedTimeoutsProvider(5.Seconds(), 3.Seconds(), 10.Seconds());
            var budget = Budget.WithRemaining(4.Seconds());

            provider.GetTimeout(request, budget, 0, 5).Should().Be(4.Seconds());
            provider.GetTimeout(request, budget, 1, 5).Should().Be(3.Seconds());
            provider.GetTimeout(request, budget, 2, 5).Should().Be(4.Seconds());
        }

        [Test]
        public void Should_correctly_implement_whole_remaining_budget_tail_behaviour()
        {
            var provider = new FixedTimeoutsProvider(5.Seconds(), 3.Seconds(), 10.Seconds());
            var budget = Budget.WithRemaining(15.Seconds());

            provider.GetTimeout(request, budget, 3, 5).Should().Be(15.Seconds());
            provider.GetTimeout(request, budget, 4, 5).Should().Be(15.Seconds());
        }

        [Test]
        public void Should_correctly_implement_last_value_tail_behaviour()
        {
            var provider = new FixedTimeoutsProvider(TailTimeoutBehaviour.RepeatLastValue, 5.Seconds(), 3.Seconds(), 10.Seconds());
            var budget = Budget.WithRemaining(15.Seconds());

            provider.GetTimeout(request, budget, 3, 5).Should().Be(10.Seconds());
            provider.GetTimeout(request, budget, 4, 5).Should().Be(10.Seconds());
        }

        [Test]
        public void Should_limit_last_value_tail_timeout_by_remaining_budget()
        {
            var provider = new FixedTimeoutsProvider(TailTimeoutBehaviour.RepeatLastValue, 5.Seconds(), 3.Seconds(), 7.Seconds());
            var budget = Budget.WithRemaining(6.Seconds());

            provider.GetTimeout(request, budget, 3, 5).Should().Be(6.Seconds());
            provider.GetTimeout(request, budget, 4, 5).Should().Be(6.Seconds());
        }
    }
}
