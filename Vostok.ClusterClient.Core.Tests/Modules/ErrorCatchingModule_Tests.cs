using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class ErrorCatchingModule_Tests
    {
        private IRequestContext context;
        private ILog log;
        private ErrorCatchingModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Log.Returns(log = Substitute.For<ILog>());
            context.Request.Returns(Request.Get("foo/bar"));
            log.IsEnabledFor(default).ReturnsForAnyArgs(true);
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

            log.Received(1, LogLevel.Error);
        }

        [Test]
        public void Should_not_log_an_error_message_if_next_module_throws_a_cancellation_error()
        {
            module.ExecuteAsync(context, _ => throw new OperationCanceledException()).GetAwaiter().GetResult();

            log.Received(0, LogLevel.Error);
        }

        [Test]
        public void Should_delegate_to_next_module_if_no_exceptions_arise()
        {
            var task = Task.FromResult(ClusterResult.ReplicasNotFound(context.Request));

            module.ExecuteAsync(context, _ => task).Result.Should().BeSameAs(task.Result);
        }
    }
}