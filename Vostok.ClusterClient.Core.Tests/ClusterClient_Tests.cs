using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Topology;
using Vostok.ClusterClient.Abstractions.Transforms;
using Vostok.ClusterClient.Abstractions.Transport;
using Vostok.ClusterClient.Core.Tests.Helpers;
using Vostok.ClusterClient.Core.Topology;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.ClusterClient.Core.Transport;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.ClusterClient.Core.Tests
{
    [TestFixture]
    internal class ClusterClient_Tests
    {
        private ILog log;

        [SetUp]
        public void TestSetup()
        {
            log = new ConsoleLog();
        }

        [Test]
        public void Ctor_should_throw_an_error_when_created_with_incorrect_configuration()
        {
            Action action = () => new ClusterClient(log, _ => { });

            action.Should().Throw<ClusterClientException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_use_cluser_provider_as_is_when_there_is_no_replicas_transform()
        {
            var clusterProvider = Substitute.For<IClusterProvider>();

            var clusterClient = new ClusterClient(log, config =>
            {
                config.ClusterProvider = clusterProvider;
                config.Transport = Substitute.For<ITransport>();
            });

            clusterClient.ClusterProvider.Should().BeSameAs(clusterProvider);
        }

        [Test]
        public void Should_wrap_cluser_provider_with_transforming_facade_if_there_is_a_replicas_transform()
        {
            var clusterClient = new ClusterClient(log, config =>
            {
                config.ClusterProvider = Substitute.For<IClusterProvider>();
                config.Transport = Substitute.For<ITransport>();
                config.ReplicaTransform = Substitute.For<IReplicaTransform>();
            });

            clusterClient.ClusterProvider.Should().BeOfType<TransformingClusterProvider>();
        }
    }
}