using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Modules;
using Vostok.ClusterClient.Abstractions.Transport;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class LeakPreventionModule_Tests
    {
        private IRequestContext context;
        private ClusterResult result;
        private LeakPreventionModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Transport.Returns(Substitute.For<ITransport>());

            result = new ClusterResult(ClusterResultStatus.Canceled, new List<ReplicaResult>(), null, Request.Get(""));
            module = new LeakPreventionModule();
        }

        [Test]
        public void Should_return_upstream_cluster_result_as_is()
        {
            Execute().Should().BeSameAs(result);
        }

        [Test]
        public void Should_wrap_transport_in_context_with_leak_prevention_decorator()
        {
            Execute();

            context.Received().Transport = Arg.Any<LeakPreventionTransport>();
        }

        private ClusterResult Execute()
        {
            return module.ExecuteAsync(context, _ => Task.FromResult(result)).GetAwaiter().GetResult();
        }
    }
}