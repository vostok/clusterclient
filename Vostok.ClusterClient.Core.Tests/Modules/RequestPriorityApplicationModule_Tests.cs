using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestPriorityApplicationModule_Tests
    {
        private IRequestContext context;
        private RequestPriorityModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Request.Returns(Request.Get("foo/bar"));
            context.Parameters.Returns(RequestParameters.Empty);

            module = new RequestPriorityModule();
        }

        [Test]
        public void Should_not_touch_request_if_priority_is_null()
        {
            Execute();

            context.DidNotReceive().Request = Arg.Any<Request>();
        }

        [TestCase(RequestPriority.Critical)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(RequestPriority.Sheddable)]
        public void Should_set_priority_to_headers(RequestPriority priority)
        {
            context.Parameters.Returns(new RequestParameters(priority: priority));

            Request request= null;
            context.When(x => x.Request = Arg.Any<Request>()).Do(x => request = x.Arg<Request>());
            Execute();

            context.Received(1).Request = Arg.Any<Request>();
            request.Headers[HeaderNames.RequestPriority].Should().Be(priority.ToString());
        }

        private void Execute()
        {
            var taskSource = new TaskCompletionSource<ClusterResult>();

            var task = taskSource.Task;

            module.ExecuteAsync(context, _ => task).Should().BeSameAs(task);
        }
    }
}