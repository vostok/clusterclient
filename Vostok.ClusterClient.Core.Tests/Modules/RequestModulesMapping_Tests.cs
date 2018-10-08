using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    internal class RequestModulesMapping_Tests
    {
        [Test]
        public void Should_map_all_RequestModule_enum_values_to_different_module_types()
        {
            var enumValues = (RequestModule[]) Enum.GetValues(typeof(RequestModule));
            var types = enumValues.Select(RequestModulesMapping.GetModuleType).ToArray();
            types.Distinct().Should().BeEquivalentTo(types);
            types.All(x => typeof(IRequestModule).IsAssignableFrom(x)).Should().BeTrue();
        }
        
        [Test]
        public void Should_know_all_request_module_types()
        {
            var expectedTypes = typeof(IRequestModule).Assembly.GetTypes().Where(typeof(IRequestModule).IsAssignableFrom).ToArray();
            var enumValues = (RequestModule[]) Enum.GetValues(typeof(RequestModule));
            var actualTypes = enumValues.Select(RequestModulesMapping.GetModuleType).ToArray();
            actualTypes.Should().BeEquivalentTo(expectedTypes);
        }
    }
}