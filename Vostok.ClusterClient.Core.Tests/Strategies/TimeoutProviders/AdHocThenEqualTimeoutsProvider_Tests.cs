using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies.TimeoutProviders;
using Vostok.ClusterClient.Core.Tests.Helpers;

namespace Vostok.ClusterClient.Core.Tests.Strategies.TimeoutProviders
{
    [TestFixture]
    internal class AdHocThenEqualTimeoutsProvider_Tests
    {
        [Test]
        public void Should_return_set_up_timeouts_first_and_then_use_equal_division()
        {
            var provider = new AdHocThenEqualTimeoutsProvider(3, () => 20.Seconds(), () => 12.Seconds(), () => 17.Seconds());
            var budget = Budget.WithRemaining(600.Seconds());
            var request = Request.Get("/foo");

            provider.GetTimeout(request, budget, 0, 10).Should().Be(20.Seconds());
            provider.GetTimeout(request, budget, 1, 10).Should().Be(12.Seconds());
            provider.GetTimeout(request, budget, 2, 10).Should().Be(17.Seconds());
            provider.GetTimeout(request, budget, 3, 10).Should().Be(200.Seconds());
            provider.GetTimeout(request, budget, 4, 10).Should().Be(300.Seconds());
            provider.GetTimeout(request, budget, 5, 10).Should().Be(600.Seconds());
            provider.GetTimeout(request, budget, 6, 10).Should().Be(600.Seconds());
        }
    }
}
