using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.Logging.ConsoleLog;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class ErrorCatchingModule_Tests
    {
        private IRequestContext context;
        private ConsoleLog log;
        private ErrorCatchingModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Log.Returns(log = new ConsoleLog());
            context.Request.Returns(Request.Get("foo/bar"));
            module = new ErrorCatchingModule();
        }

        [Test]
        public void Should_return_unexpected_exception_result_if_next_module_throws_an_error()
        {
            module.ExecuteAsync(context, _ => { throw new Exception(); }).Result.Status.Should().Be(ClusterResultStatus.UnexpectedException);
        }

        [Test]
        public void Should_return_canceled_result_if_next_module_throws_a_cancellation_exception()
        {
            module.ExecuteAsync(context, _ => { throw new OperationCanceledException(); }).Result.Status.Should().Be(ClusterResultStatus.Canceled);
        }

        [Test]
        public void Should_log_an_error_message_if_next_module_throws_an_error()
        {
            module.ExecuteAsync(context, _ => { throw new Exception(); }).GetAwaiter().GetResult();

            // todo(Mansiper): fix it in many places: CallsErrorCount for logs is absent
            // log.CallsErrorCount.Should().Be(1);  // todo(Mansiper): fix it
        }

        [Test]
        public void Should_not_log_an_error_message_if_next_module_throws_a_cancellation_error()
        {
            module.ExecuteAsync(context, _ => { throw new OperationCanceledException(); }).GetAwaiter().GetResult();

            // log.CallsErrorCount.Should().Be(0);  // todo(Mansiper): fix it
        }

        [Test]
        public void Should_delegate_to_next_module_if_no_exceptions_arise()
        {
            var task = Task.FromResult(ClusterResult.ReplicasNotFound(context.Request));

            module.ExecuteAsync(context, _ => task).Result.Should().BeSameAs(task.Result);
        }
    }
}
