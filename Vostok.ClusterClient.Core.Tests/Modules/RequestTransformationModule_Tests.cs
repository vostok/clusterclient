using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Logging.Console;

// ReSharper disable PossibleNullReferenceException

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestTransformationModule_Tests
    {
        private Request request1;
        private Request request2;
        private Request request3;
        private Request request4;
        private Request request5;
        private RequestContext context;
        private RequestTransformationModule module;

        private IRequestTransform transform1;
        private IRequestTransform transform2;
        private IAsyncRequestTransform transform3;
        private IAsyncRequestTransform transform4;

        private List<IRequestTransformMetadata> transforms;

        [SetUp]
        public void TestSetup()
        {
            request1 = Request.Get("/1");
            request2 = Request.Get("/2");
            request3 = Request.Get("/3");
            request4 = Request.Get("/4");
            request5 = Request.Get("/5");

            context = new RequestContext(
                request1,
                new RequestParameters(Strategy.SingleReplica),
                Budget.Infinite,
                new ConsoleLog(),
                clusterProvider: default,
                replicaOrdering: default,
                transport: default,
                maximumReplicasToUse: int.MaxValue,
                connectionAttempts: default);

            transform1 = Substitute.For<IRequestTransform>();
            transform1.Transform(Arg.Any<Request>()).Returns(_ => request2);

            transform2 = Substitute.For<IRequestTransform>();
            transform2.Transform(Arg.Any<Request>()).Returns(_ => request3);

            transform3 = Substitute.For<IAsyncRequestTransform>();
            transform3.TransformAsync(Arg.Any<Request>()).Returns(_ => request4);

            transform4 = Substitute.For<IAsyncRequestTransform>();
            transform4.TransformAsync(Arg.Any<Request>()).Returns(_ => request5);

            transforms = new List<IRequestTransformMetadata> {transform1, transform2, transform3, transform4};

            module = new RequestTransformationModule(transforms);
        }

        [Test]
        public void Should_not_modify_request_if_transforms_list_is_null()
        {
            module = new RequestTransformationModule(null);

            Execute();

            context.Request.Should().BeSameAs(request1);
        }

        [Test]
        public void Should_not_modify_request_if_transforms_list_is_empty()
        {
            transforms.Clear();

            Execute();

            context.Request.Should().BeSameAs(request1);
        }

        [Test]
        public void Should_apply_all_request_transforms_in_order()
        {
            Execute();

            context.Request.Should().BeSameAs(request5);

            Received.InOrder(
                () =>
                {
                    transform1.Transform(request1);
                    transform2.Transform(request2);
                    transform3.TransformAsync(request3);
                    transform4.TransformAsync(request4);
                });
        }

        [Test]
        public void Should_substitute_request_content_stream_for_a_single_use_implementation()
        {
            request5 = request5.WithContent(Stream.Null, 123L);

            Execute();

            context.Request.StreamContent.Should().BeOfType<SingleUseStreamContent>();
            context.Request.StreamContent.Stream.Should().BeSameAs(Stream.Null);
            context.Request.StreamContent.Length.Should().Be(123L);
        }

        private void Execute()
        {
            module.ExecuteAsync(context, _ => Task.FromResult<ClusterResult>(null))
                .GetAwaiter()
                .GetResult()
                .Should()
                .BeSameAs(null);
        }
    }
}