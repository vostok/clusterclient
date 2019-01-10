using System;
using System.Threading.Tasks;
using FluentAssertions;
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

        private string priorityHeader;
        private string identityHeader;

        [SetUp]
        public void TestSetup()
        {
            context = new RequestContext(
                Request.Get("foo/bar"), 
                RequestParameters.Empty, 
                default,
                default,
                default,
                default,
                Guid.NewGuid().ToString());

            module = new AuxiliaryHeadersModule(
                priorityHeader = Guid.NewGuid().ToString(),
                identityHeader = Guid.NewGuid().ToString());
        }

        [TestCase(RequestPriority.Critical)]
        [TestCase(RequestPriority.Ordinary)]
        [TestCase(RequestPriority.Sheddable)]
        public void Should_set_request_priority_to_headers(RequestPriority priority)
        {
            context.Parameters = context.Parameters.WithPriority(priority);

            Execute();

            context.Request.Headers?[priorityHeader].Should().Be(priority.ToString());
        }
        
        [Test]
        public void Should_set_client_application_name_to_headers()
        {
            Execute();

            context.Request.Headers?[identityHeader].Should().Be(context.ClientApplicationName);
        }

        private void Execute()
        {
            var taskSource = new TaskCompletionSource<ClusterResult>();

            var task = taskSource.Task;

            module.ExecuteAsync(context, _ => task).Should().BeSameAs(task);
        }
    }
}