using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Strategies;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    internal class RequestParameters_Tests
    {
        private readonly RequestParameters parameters = RequestParameters.Empty;
        
        [Test]
        public void WithStrategy_Should_return_updated_parameters_with_updated_strategy()
        {
            var newParameters = parameters.WithStrategy(Strategy.Sequential3);
            newParameters.Should().NotBeSameAs(parameters);
            newParameters.Strategy.Should().Be(Strategy.Sequential3);
        }
        
        [TestCase(RequestPriority.Critical)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(RequestPriority.Sheddable)]
        public void WithPriority_Should_return_updated_parameters_with_updated_priority(RequestPriority priority)
        {
            var newParameters = parameters.WithPriority(priority);
            newParameters.Should().NotBeSameAs(parameters);
            newParameters.Priority.Should().Be(priority);
        }
        
        [TestCase("key", 1)]
        [TestCase("xyz", "abc")]
        public void WithProperty_Should_return_updated_parameters_with_updated_properties(string key, object value)
        {
            var newParameters = parameters.WithProperty(key, value);
            newParameters.Should().NotBeSameAs(parameters);
            newParameters.Properties[key].Should().Be(value);
        }
    }
}