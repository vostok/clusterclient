using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Transforms;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Modules;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Tests.Helpers;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.Logging.Console;

// ReSharper disable PossibleNullReferenceException

namespace Vostok.ClusterClient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestTransformationModule_Tests
    {
        private Request request1;
        private Request request2;
        private Request request3;
        private RequestContext context;
        private RequestTransformationModule module;

        private IRequestTransform transform1;
        private IRequestTransform transform2;
        private List<IRequestTransform> transforms;

        [SetUp]
        public void TestSetup()
        {
            request1 = Request.Get("/1");
            request2 = Request.Get("/2");
            request3 = Request.Get("/3");

            context = new RequestContext(request1, Strategy.SingleReplica, Budget.Infinite, new ConsoleLog(), null, null, int.MaxValue);

            transform1 = Substitute.For<IRequestTransform>();
            transform1.Transform(Arg.Any<Request>()).Returns(_ => request2);

            transform2 = Substitute.For<IRequestTransform>();
            transform2.Transform(Arg.Any<Request>()).Returns(_ => request3);

            transforms = new List<IRequestTransform> {transform1, transform2};

            module = new RequestTransformationModule(transforms);
        }

        [Test]
        public void Should_not_modify_request_if_transforms_list_is_null()
        {
            module = new RequestTransformationModule(null);

            Execute();

            AssertionExtensions.Should((object)context.Request).BeSameAs(request1);
        }

        [Test]
        public void Should_not_modify_request_if_transforms_list_is_empty()
        {
            transforms.Clear();

            Execute();

            AssertionExtensions.Should((object)context.Request).BeSameAs(request1);
        }

        [Test]
        public void Should_apply_all_request_transforms_in_order()
        {
            Execute();

            AssertionExtensions.Should((object)context.Request).BeSameAs(request3);

            Received.InOrder(() =>
            {
                transform1.Transform(request1);
                transform2.Transform(request2);
            });
        }

        [Test]
        public void Should_substitute_request_content_stream_for_a_single_use_implementation()
        {
            request3 = request3.WithContent(Stream.Null, 123L);

            Execute();

            AssertionExtensions.Should((object)context.Request.StreamContent).BeOfType<SingleUseStreamContent>();
            AssertionExtensions.Should((object)context.Request.StreamContent.Stream).BeSameAs(Stream.Null);
            AssertionExtensions.Should((long?)context.Request.StreamContent.Length).Be(123L);
        }

        private void Execute()
        {
            var taskSource = new TaskCompletionSource<ClusterResult>();

            var task = taskSource.Task;

            AssertionExtensions.Should((object)module.ExecuteAsync(context, _ => task)).BeSameAs(task);
        }
    }
}
