using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    public class RequestModules_Tests
    {
        [Test]
        public void Members_of_RequestModules_should_be_ordered_in_pipeline_order()
        {
            var configuration = new ClusterClientConfiguration(new SilentLog());

            configuration.TargetServiceName = string.Empty;
            
            configuration.SetupAdaptiveThrottling();
            configuration.SetupReplicaBudgeting();
            configuration.SetupHttpMethodValidation();

            var chain = RequestModuleChainBuilder.BuildChain(configuration, new PerInstanceReplicaStorageProvider());

            var values = (RequestModule[]) Enum.GetValues(typeof(RequestModule));

            values.Should().HaveCount(chain.Count);
            
            for (var i = 0; i < values.Length; i++)
                RequestModulesMapping.GetModuleType(values[i]).Should().Be(chain[i].GetType());
        }
    }
}