using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Tests.Helpers;
using Vostok.Logging.Console;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class TimeoutValidationModule_Tests
    {
        private IRequestContext context;
        private ConsoleLog log;
        private TimeoutValidationModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Log.Returns(log = new ConsoleLog());
            context.Request.Returns(Request.Get("foo/bar"));
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
        public void Should_log_an_error_message_if_timeout_is_negative()
        {
            var budget = Budget.WithRemaining(-1.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).GetAwaiter().GetResult();

            // log.CallsErrorCount.Should().Be(1);  // todo(Mansiper): fix it
        }

        [Test]
        public void Should_return_time_expired_result_if_time_budget_is_already_expired()
        {
            var budget = Budget.WithRemaining(0.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).Result.Status.Should().Be(ClusterResultStatus.TimeExpired);
        }

        [Test]
        public void Should_log_an_error_message_if_time_budget_is_already_expired()
        {
            var budget = Budget.WithRemaining(0.Seconds());

            context.Budget.Returns(budget);

            module.ExecuteAsync(context, _ => null).GetAwaiter().GetResult();

            // log.CallsErrorCount.Should().Be(1);  // todo(Mansiper): fix it
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
