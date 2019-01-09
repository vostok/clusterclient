using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Strategies;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    internal class RequestParameters_Tests
    {
        private readonly RequestParameters parameters = new RequestParameters(Strategy.Forking3, RequestPriority.Sheddable)
            .WithProperty("a", "a")
            .WithProperty("b", 1);

        [Test]
        public void Empty_parameters_instance_should_have_no_strategy()
        {
            RequestParameters.Empty.Strategy.Should().BeNull();
        }

        [Test]
        public void Empty_parameters_instance_should_have_empty_properties()
        {
            RequestParameters.Empty.Properties.Should().BeEmpty();
        }

        [Test]
        public void WithStrategy_Should_return_updated_parameters_with_updated_strategy()
        {
            var newParameters = parameters.WithStrategy(Strategy.Sequential3);
            newParameters.Should().NotBeSameAs(parameters);
            newParameters.Strategy.Should().Be(Strategy.Sequential3);
        }

        [Test]
        public void WithStrategy_Should_preserve_priority_and_properties()
        {
            var newParameters = parameters.WithStrategy(Strategy.Sequential3);
            newParameters.Properties.Should().BeEquivalentTo(parameters.Properties);
            newParameters.Priority.Should().Be(parameters.Priority);
        }
        
        [Test]
        public void WithStrategy_Should_return_this_if_strategy_has_not_changed()
        {
            var newParameters = parameters.WithStrategy(parameters.Strategy);
            newParameters.Should().BeSameAs(parameters);
        }

        [TestCase(RequestPriority.Critical)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(null)]
        public void WithPriority_Should_return_updated_parameters_with_updated_priority(RequestPriority? priority)
        {
            var newParameters = parameters.WithPriority(priority);
            newParameters.Should().NotBeSameAs(parameters);
            newParameters.Priority.Should().Be(priority);
        }

        [Test]
        public void WithPriority_Should_preserve_strategy_and_properties()
        {
            var newParameters = parameters.WithPriority(RequestPriority.Critical);
            newParameters.Properties.Should().BeEquivalentTo(parameters.Properties);
            newParameters.Strategy.Should().BeSameAs(parameters.Strategy);
        }
        
        [Test]
        public void WithPriority_Should_return_this_if_priority_has_not_changed()
        {
            var newParameters = parameters.WithPriority(parameters.Priority);
            newParameters.Should().BeSameAs(parameters);
        }

        [TestCase("key", 1)]
        [TestCase("xyz", "abc")]
        [TestCase("a", "b")]
        public void WithProperty_Should_return_updated_parameters_with_updated_properties(string key, object value)
        {
            var newParameters = parameters.WithProperty(key, value);
            newParameters.Should().NotBeSameAs(parameters);
            newParameters.Properties[key].Should().Be(value);
        }

        [Test]
        public void WithProperty_Should_preserve_strategy_parameters_and_properties()
        {
            var newParameters = parameters.WithProperty("c", 2);
            newParameters.Strategy.Should().BeSameAs(parameters.Strategy);
            newParameters.Priority.Should().Be(parameters.Priority);
            newParameters.Properties.Should().Contain(parameters.Properties);
        }
        
        [Test]
        public void WithProperty_Should_return_this_if_property_has_not_changed()
        {
            var newParameters = parameters.WithProperty("a", parameters.Properties["a"]);
            newParameters.Should().BeSameAs(parameters);
        }
    }
}