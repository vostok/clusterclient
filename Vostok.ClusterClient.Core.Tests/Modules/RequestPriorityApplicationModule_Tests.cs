using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class AuxiliaryHeadersModule_Tests
    {
        private IRequestContext context;
        private AuxiliaryHeadersModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Request.Returns(Request.Get("foo/bar"));
            context.Parameters.Returns(RequestParameters.Empty);

            module = new AuxiliaryHeadersModule(null);
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

            Request request = null;
            context.When(x => x.Request = Arg.Any<Request>()).Do(x => request = x.Arg<Request>());
            Execute();

            context.Received(1).Request = Arg.Any<Request>();
            request.Headers[HeaderNames.RequestPriority].Should().Be(priority.ToString());
        }
        
        [TestCase(RequestPriority.Critical)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(RequestPriority.Sheddable)]
        public void Should_set_default_priority_to_headers(RequestPriority priority)
        {
            module = new AuxiliaryHeadersModule(priority);
            
            context.Parameters.Returns(RequestParameters.Empty);

            Request request = null;
            context.When(x => x.Request = Arg.Any<Request>()).Do(x => request = x.Arg<Request>());
            Execute();

            context.Received(1).Request = Arg.Any<Request>();
            request.Headers[HeaderNames.RequestPriority].Should().Be(priority.ToString());
        }
        
        [TestCase("qwe")]
        [TestCase("xyz")]
        public void Should_add_client_app_name_from_context(string name)
        {
            context.ClientApplicationName.Returns(name);

            module.ExecuteAsync(
                context,
                requestContext =>
                {
                    requestContext.Request.Headers[HeaderNames.ApplicationIdentity].Should().Be(name);
                    return null;
                });
        }

        private void Execute()
        {
            var taskSource = new TaskCompletionSource<ClusterResult>();

            var task = taskSource.Task;

            module.ExecuteAsync(context, _ => task).Should().BeSameAs(task);
        }
    }
}