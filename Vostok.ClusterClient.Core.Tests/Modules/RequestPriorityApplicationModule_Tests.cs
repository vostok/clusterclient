using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.Context;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestPriorityApplicationModule_Tests
    {
        private IRequestContext context;
        private RequestPriorityApplicationModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Request.Returns(Request.Get("foo/bar"));
            context.Priority.Returns(null as RequestPriority?);

            module = new RequestPriorityApplicationModule();
        }

        [TearDown]
        public void TestTeardown()
        {
            // FlowingContextProvider.Set(null as RequestPriority?);    // todo(Mansiper): fix it
        }

        [Test]
        public void Should_not_touch_request_if_priority_is_null()
        {
            Execute();

            context.DidNotReceive().Request = Arg.Any<Request>();
        }

        [TestCase(RequestPriority.Sheddable)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(RequestPriority.Critical)]
        public void Should_add_a_priority_header_if_priority_is_not_null(RequestPriority priority)
        {
            context.Priority.Returns(priority);

            Execute();

            context.Received().Request = Arg.Is<Request>(r => r.Headers[HeaderNames.XKonturRequestPriority] == priority.ToString());
        }

        [TestCase(RequestPriority.Sheddable)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(RequestPriority.Critical)]
        public void Should_add_a_priority_header_if_priority_is_null_but_there_is_a_value_in_context(RequestPriority priority)
        {
            FlowingContext.Properties.Set("request.priority", priority as RequestPriority?);

            Execute();

            context.Received().Request = Arg.Is<Request>(r => r.Headers[HeaderNames.XKonturRequestPriority] == priority.ToString());

            FlowingContext.Properties.Remove("request.priority");
        }

        private void Execute()
        {
            var taskSource = new TaskCompletionSource<ClusterResult>();

            var task = taskSource.Task;

            module.ExecuteAsync(context, _ => task).Should().BeSameAs(task);
        }
    }
}