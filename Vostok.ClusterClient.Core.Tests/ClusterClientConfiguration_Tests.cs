using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Core.Tests
{
    [TestFixture]
    internal class ClusterClientConfiguration_Tests
    {
        private ILog log;
        private ClusterClientConfiguration configuration;

        [SetUp]
        public void TestSetup()
        {
            log = new ConsoleLog();

            configuration = new ClusterClientConfiguration(log)
            {
                Transport = Substitute.For<ITransport>(),
                ClusterProvider = Substitute.For<IClusterProvider>()
            };
        }

        [Test]
        public void Should_initially_have_null_transport()
        {
            configuration = new ClusterClientConfiguration(log);

            configuration.Transport.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_null_cluster_provider()
        {
            configuration = new ClusterClientConfiguration(log);

            configuration.ClusterProvider.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_empty_replica_filters_list()
        {
            configuration.ReplicasFilters.Should().BeEmpty();
        }

        [Test]
        public void Should_initially_have_null_replica_transform()
        {
            configuration.ReplicaTransform.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_null_replica_ordering()
        {
            configuration.ReplicaOrdering.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_default_replica_storage_scope()
        {
            configuration.ReplicaStorageScope.Should().Be(ClusterClientDefaults.ReplicaStorageScope);
        }

        [Test]
        public void Should_initially_have_empty_request_transforms_list()
        {
            configuration.RequestTransforms.Should().BeEmpty();
        }

        [Test]
        public void Should_initially_have_empty_response_transforms_list()
        {
            configuration.ResponseTransforms.Should().BeEmpty();
        }

        [Test]
        public void Should_initially_have_empty_response_criteria_list()
        {
            configuration.ResponseCriteria.Should().BeEmpty();
        }

        [Test]
        public void Should_initially_have_empty_modules_collection()
        {
            configuration.Modules.Should().BeEmpty();
        }

        [Test]
        public void Should_initially_have_null_retry_policy()
        {
            configuration.RetryPolicy.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_null_retry_strategy()
        {
            configuration.RetryStrategy.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_null_response_selector()
        {
            configuration.ResponseSelector.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_null_default_request_strategy()
        {
            configuration.DefaultRequestStrategy.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_default_request_timeout()
        {
            configuration.DefaultTimeout.Should().Be(ClusterClientDefaults.Timeout);
        }

        [Test]
        public void Should_initially_have_default_connection_timeout()
        {
            configuration.DefaultConnectionTimeout.Should().Be(ClusterClientDefaults.ConnectionTimeout);
        }

        [Test]
        public void Should_initially_have_null_request_priority()
        {
            configuration.DefaultPriority.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_all_logging_options_enabled()
        {
            configuration.Logging.LogRequestDetails.Should().BeTrue();
            configuration.Logging.LogResultDetails.Should().BeTrue();
            configuration.Logging.LogReplicaRequests.Should().BeTrue();
            configuration.Logging.LogReplicaResults.Should().BeTrue();
        }

        [Test]
        public void Should_initially_have_default_max_replicas_per_request()
        {
            configuration.MaxReplicasUsedPerRequest.Should().Be(ClusterClientDefaults.MaxReplicasUsedPerRequest);
        }

        [Test]
        public void Validate_should_produce_no_errors_on_a_valid_configuration()
        {
            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_response_criteria_list_is_null()
        {
            configuration.ResponseCriteria = null;

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_request_transforms_list_is_null()
        {
            configuration.RequestTransforms = null;

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_replica_filters_list_is_null()
        {
            configuration.ReplicasFilters = null;

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_response_transforms_list_is_null()
        {
            configuration.ResponseTransforms = null;

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_request_modules_list_is_null()
        {
            configuration.Modules = null;

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_last_response_criteria_is_always_accept()
        {
            configuration.SetupResponseCriteria(new AlwaysAcceptCriterion());

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_no_errors_when_last_response_criteria_is_always_reject()
        {
            configuration.SetupResponseCriteria(new AlwaysRejectCriterion());

            configuration.Validate().Should().BeEmpty();
        }

        [Test]
        public void Validate_should_produce_an_error_if_transport_implementation_is_not_set()
        {
            configuration.Transport = null;

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_cluster_provider_implementation_is_not_set()
        {
            configuration.ClusterProvider = null;

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_last_response_criterion_is_not_always_accept_or_reject()
        {
            configuration.SetupResponseCriteria(new RejectNetworkErrorsCriterion());

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_response_criteria_list_contains_a_null_value()
        {
            configuration.SetupResponseCriteria(null, new AlwaysAcceptCriterion());

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_request_transforms_list_contains_a_null_value()
        {
            configuration.AddRequestTransform(null as IRequestTransform);

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_response_transforms_list_contains_a_null_value()
        {
            configuration.AddResponseTransform(null as IResponseTransform);

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_request_modules_list_contains_a_null_value()
        {
            configuration.AddRequestModule(null);

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_default_timeout_is_negative()
        {
            configuration.DefaultTimeout = -1.Seconds();

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_default_connection_timeout_is_negative()
        {
            configuration.DefaultConnectionTimeout = -1.Seconds();

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_default_timeout_is_zero()
        {
            configuration.DefaultTimeout = TimeSpan.Zero;

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_max_replicas_used_per_request_is_negative()
        {
            configuration.MaxReplicasUsedPerRequest = -1;

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_max_replicas_used_per_request_is_zero()
        {
            configuration.MaxReplicasUsedPerRequest = 0;

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void Validate_should_produce_an_error_if_connection_attempts_count_is_negative()
        {
            configuration.ConnectionAttempts = -1;

            Console.Out.WriteLine(configuration.Validate().Should().ContainSingle().Which);
        }

        [Test]
        public void IsValid_should_return_true_for_valid_configuration()
        {
            configuration.IsValid.Should().BeTrue();
        }

        [Test]
        public void IsValid_should_return_false_for_invalid_configuration()
        {
            configuration.Transport = null;

            configuration.IsValid.Should().BeFalse();
        }

        [Test]
        public void ValidateOrDie_should_not_throw_an_error_when_called_on_valid_configuration()
        {
            Action action = () => configuration.ValidateOrDie();

            action.Should().NotThrow();
        }

        [Test]
        public void ValidateOrDie_should_throw_an_error_when_called_on_invalid_configuration()
        {
            configuration.Transport = null;
            configuration.ClusterProvider = null;
            configuration.DefaultTimeout = TimeSpan.Zero;

            Action action = () => configuration.ValidateOrDie();

            action.Should().Throw<ClusterClientException>().Which.ShouldBePrinted();
        }

        [Test]
        public void AugmentWithDefaults_should_not_set_transport_implementation()
        {
            configuration.Transport = null;

            configuration.AugmentWithDefaults();

            configuration.Transport.Should().BeNull();
        }

        [Test]
        public void AugmentWithDefaults_should_not_set_cluster_provider_implementation()
        {
            configuration.ClusterProvider = null;

            configuration.AugmentWithDefaults();

            configuration.ClusterProvider.Should().BeNull();
        }

        [Test]
        public void AugmentWithDefaults_should_not_set_replica_transform_implementation()
        {
            configuration.AugmentWithDefaults();

            configuration.ReplicaTransform.Should().BeNull();
        }

        [Test]
        public void AugmentWithDefaults_should_set_replica_ordering_implementation_if_current_is_null()
        {
            configuration.AugmentWithDefaults();

            configuration.ReplicaOrdering.Should().NotBeNull();
        }

        [Test]
        public void AugmentWithDefaults_should_not_rewrite_existing_replica_ordering()
        {
            var original = configuration.ReplicaOrdering = Substitute.For<IReplicaOrdering>();

            configuration.AugmentWithDefaults();

            configuration.ReplicaOrdering.Should().BeSameAs(original);
        }

        [Test]
        public void AugmentWithDefaults_should_not_add_any_request_transforms()
        {
            configuration.AugmentWithDefaults();

            configuration.RequestTransforms.Should().BeEmpty();
        }

        [Test]
        public void AugmentWithDefaults_should_not_add_any_replica_filters()
        {
            configuration.AugmentWithDefaults();

            configuration.ReplicasFilters.Should().BeEmpty();
        }

        [Test]
        public void AugmentWithDefaults_should_not_add_any_response_transforms()
        {
            configuration.AugmentWithDefaults();

            configuration.ResponseTransforms.Should().BeEmpty();
        }

        [Test]
        public void AugmentWithDefaults_should_not_add_any_request_modules()
        {
            configuration.AugmentWithDefaults();

            configuration.Modules.Should().BeEmpty();
        }

        [Test]
        public void AugmentWithDefaults_should_set_default_response_criteria_if_current_list_is_null()
        {
            configuration.ResponseCriteria = null;

            configuration.AugmentWithDefaults();

            var defaultCriteria = ClusterClientDefaults.ResponseCriteria();

            configuration.ResponseCriteria.Should().HaveSameCount(defaultCriteria);

            for (var i = 0; i < configuration.ResponseCriteria.Count; i++)
            {
                configuration.ResponseCriteria[i].Should().BeOfType(defaultCriteria[i].GetType());
            }
        }

        [Test]
        public void AugmentWithDefaults_should_set_default_response_criteria_if_current_list_is_empty()
        {
            configuration.AugmentWithDefaults();

            var defaultCriteria = ClusterClientDefaults.ResponseCriteria();

            configuration.ResponseCriteria.Should().HaveSameCount(defaultCriteria);

            for (var i = 0; i < configuration.ResponseCriteria.Count; i++)
            {
                configuration.ResponseCriteria[i].Should().BeOfType(defaultCriteria[i].GetType());
            }
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_response_criteria()
        {
            configuration.SetupResponseCriteria(new RejectNetworkErrorsCriterion(), new AlwaysAcceptCriterion());

            var before = configuration.ResponseCriteria;

            configuration.AugmentWithDefaults();

            configuration.ResponseCriteria.Should().Equal(before);
        }

        [Test]
        public void AugmentWithDefaults_should_set_retry_policy_if_not_set_yet()
        {
            configuration.AugmentWithDefaults();

            configuration.RetryPolicy.Should().BeSameAs(ClusterClientDefaults.RetryPolicy);
        }

        [Test]
        public void AugmentWithDefaults_should_set_retry_strategy_if_not_set_yet()
        {
            configuration.AugmentWithDefaults();

            configuration.RetryStrategy.Should().BeNull("This is deprecated module and it should be converted to RetryStrategyEx one.");
            configuration.RetryStrategyEx.Should().BeSameAs(ClusterClientDefaults.RetryStrategyEx);
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_retry_policy()
        {
            configuration.RetryPolicy = Substitute.For<IRetryPolicy>();

            var before = configuration.RetryPolicy;

            configuration.AugmentWithDefaults();

            configuration.RetryPolicy.Should().BeSameAs(before);
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_retry_strategy()
        {
            configuration.RetryStrategy = Substitute.For<IRetryStrategy>();

            var before = configuration.RetryStrategy;

            configuration.AugmentWithDefaults();

            configuration.RetryStrategy.Should().BeSameAs(before);
        }

        [Test]
        public void AugmentWithDefaults_should_set_response_selector_if_not_set_yet()
        {
            configuration.AugmentWithDefaults();

            configuration.ResponseSelector.Should().BeSameAs(ClusterClientDefaults.ResponseSelector);
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_response_selector()
        {
            configuration.ResponseSelector = Substitute.For<IResponseSelector>();

            var before = configuration.ResponseSelector;

            configuration.AugmentWithDefaults();

            configuration.ResponseSelector.Should().BeSameAs(before);
        }

        [Test]
        public void AugmentWithDefaults_should_set_default_request_strategy_if_not_set_yet()
        {
            configuration.AugmentWithDefaults();

            configuration.DefaultRequestStrategy.Should().BeSameAs(ClusterClientDefaults.RequestStrategy);
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_default_request_strategy()
        {
            configuration.DefaultRequestStrategy = Substitute.For<IRequestStrategy>();

            var before = configuration.DefaultRequestStrategy;

            configuration.AugmentWithDefaults();

            configuration.DefaultRequestStrategy.Should().BeSameAs(before);
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_default_request_timeout()
        {
            configuration.DefaultTimeout = 1.Hours();

            configuration.AugmentWithDefaults();

            configuration.DefaultTimeout.Should().Be(1.Hours());
        }

        [Test]
        public void AugmentWithDefaults_should_initialize_client_application_name_if_it_is_null()
        {
            configuration.AugmentWithDefaults();

            string.IsNullOrWhiteSpace(configuration.ClientApplicationName).Should().BeFalse();
        }

        [Test]
        public void AugmentWithDefaults_should_not_overwrite_existing_client_application_name()
        {
            configuration.ClientApplicationName = "123";

            configuration.AugmentWithDefaults();

            configuration.ClientApplicationName.Should().Be("123");
        }
    }
}