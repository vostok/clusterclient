using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class TimeoutValidationModule_Tests
    {
        private IRequestContext context;
        private ILog log;
        private TimeoutValidationModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Log.Returns(log = Substitute.For<ILog>());
            context.Request.Returns(Request.Get("foo/bar"));
            log.IsEnabledFor(default).ReturnsForAnyArgs(true);
            module = new TimeoutValidationModule();
        }

        [Test]
        public void Should_return_incorrect_arguments_result_if_timeout_is_negative()
        {
            var budget = Budget.WithRemaining(-1.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).Result.Status.Should().Be(ClusterResultStatus.IncorrectArguments);
        }

        [Test]
        public void Should_return_incorrect_arguments_result_if_timeout_is_greater_than_intMaxValue()
        {
            var budget = Budget.WithRemaining(TimeSpan.FromMilliseconds((long)int.MaxValue + 1));

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).Result.Status.Should().Be(ClusterResultStatus.IncorrectArguments);
        }

        [Test]
        public void Should_log_an_error_message_if_timeout_is_negative()
        {
            var budget = Budget.WithRemaining(-1.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).GetAwaiter().GetResult();

            log.Received(1, LogLevel.Error);
        }

        [Test]
        public void Should_return_time_expired_result_if_time_budget_is_already_expired()
        {
            var budget = Budget.WithRemaining(0.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).Result.Status.Should().Be(ClusterResultStatus.TimeExpired);
        }

        [Test]
        public void Should_log_a_warn_message_if_time_budget_is_already_expired()
        {
            var budget = Budget.WithRemaining(0.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).GetAwaiter().GetResult();

            log.Received(1, LogLevel.Warn);
        }

        [Test]
        public void Should_delegate_to_next_module_if_timeout_is_valid()
        {
            var budget = Budget.WithRemaining(5.Seconds());

            context.Budget.Returns(budget);

            var task = Task.FromResult(ClusterResult.UnexpectedException(context.Request));

            module.ExecuteAsync(context, _ => task).Should().BeSameAs(task);
        }
    }
}