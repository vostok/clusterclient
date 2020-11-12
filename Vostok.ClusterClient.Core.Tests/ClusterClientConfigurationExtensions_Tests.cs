using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Core.Tests
{
    [TestFixture]
    internal class ClusterClientConfigurationExtensions_Tests
    {
        private ClusterClientConfiguration configuration;

        [SetUp]
        public void TestSetup()
        {
            configuration = new ClusterClientConfiguration(new ConsoleLog());
        }

        [Test]
        public void SetupResponseCriteria_should_build_correct_criteria_list()
        {
            var criterion1 = Substitute.For<IResponseCriterion>();
            var criterion2 = Substitute.For<IResponseCriterion>();

            configuration.SetupResponseCriteria(criterion1, criterion2);

            configuration.ResponseCriteria.Should().Equal(criterion1, criterion2);
        }

        [Test]
        public void AddRequestModule_should_not_fail_if_modules_list_is_null()
        {
            configuration.Modules = null;

            configuration.AddRequestModule(Substitute.For<IRequestModule>());

            configuration.Modules.Should().HaveCount(1);
        }

        [Test]
        public void AddRequestModule_should_be_idempotent()
        {
            configuration.AddRequestModule(new ThreadPoolTuningModule(), RequestModule.AuxiliaryHeaders);
            configuration.AddRequestModule(new ThreadPoolTuningModule(), RequestModule.AuxiliaryHeaders);
            configuration.AddRequestModule(new ThreadPoolTuningModule(), RequestModule.AuxiliaryHeaders);
            configuration.AddRequestModule(new ThreadPoolTuningModule(), RequestModule.AuxiliaryHeaders);

            configuration.Modules[typeof(AuxiliaryHeadersModule)].Before.Should().ContainSingle().Which.Should().BeOfType<ThreadPoolTuningModule>();
        }

        [Test]
        public void AddRequestTransform_should_not_fail_if_transforms_list_is_null()
        {
            configuration.RequestTransforms = null;

            configuration.AddRequestTransform(Substitute.For<IRequestTransform>());

            configuration.RequestTransforms.Should().HaveCount(1);
        }

        [Test]
        public void AddReplicaFilter_should_not_fail_if_filters_list_is_null()
        {
            configuration.ReplicasFilters = null;

            configuration.AddReplicasFilter(Substitute.For<IReplicasFilter>());

            configuration.ReplicasFilters.Should().HaveCount(1);
        }

        [Test]
        public void AddResponseTransform_should_not_fail_if_transforms_list_is_null()
        {
            configuration.ResponseTransforms = null;

            configuration.AddResponseTransform(Substitute.For<IResponseTransform>());

            configuration.ResponseTransforms.Should().HaveCount(1);
        }
    }
}