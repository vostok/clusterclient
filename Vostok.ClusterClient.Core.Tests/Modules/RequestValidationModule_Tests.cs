using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Tests.Modules
{
    [TestFixture]
    internal class RequestValidationModule_Tests
    {
        private IRequestContext context;
        private ILog log;
        private RequestValidationModule module;

        [SetUp]
        public void TestSetup()
        {
            context = Substitute.For<IRequestContext>();
            context.Log.Returns(log = Substitute.For<ILog>());
            context.Transport.Returns(Substitute.For<ITransport>());
            context.Parameters.Returns(RequestParameters.Empty.WithStrategy(new SingleReplicaRequestStrategy()));

            log.IsEnabledFor(default).ReturnsForAnyArgs(true);

            module = new RequestValidationModule();
        }

        [Test]
        public void Should_return_incorrect_arguments_result_if_request_is_not_valid()
        {
            context.Request.Returns(CreateIncorrectRequest());

            ShouldFailChecks();
        }

        [Test]
        public void Should_log_an_error_message_when_request_is_not_valid()
        {
            context.Request.Returns(CreateIncorrectRequest());

            module.ExecuteAsync(context, _ => null).GetAwaiter().GetResult();

            log.Received(1, LogLevel.Error);
        }

        [Test]
        public void Should_delegate_to_next_module_if_request_is_valid()
        {
            context.Request.Returns(CreateCorrectRequest());

            ShouldPassChecks();
        }

        [Test]
        public void Should_fail_if_request_has_stream_content_but_transport_does_not_support_it()
        {
            context.Request.Returns(Request.Post("foo/bar").WithContent(Stream.Null, 100));

            context.Transport.Capabilities.Returns(TransportCapabilities.None);

            ShouldFailChecks();
        }

        [Test]
        public void Should_fail_if_request_has_stream_content_with_parallel_strategy()
        {
            context.Request.Returns(Request.Post("foo/bar").WithContent(Stream.Null, 100));

            context.Transport.Capabilities.Returns(TransportCapabilities.RequestStreaming);

            context.Parameters.Returns(RequestParameters.Empty.WithStrategy(new ParallelRequestStrategy(2)));

            ShouldFailChecks();
        }

        [Test]
        public void Should_allow_request_with_stream_content_when_transport_supports_it()
        {
            context.Request.Returns(Request.Post("foo/bar").WithContent(Stream.Null, 100));

            context.Transport.Capabilities.Returns(TransportCapabilities.RequestStreaming);

            ShouldPassChecks();
        }

        [Test]
        public void Should_allow_request_with_stream_content_and_parallel_strategy_without_actual_parallelism()
        {
            context.Request.Returns(Request.Post("foo/bar").WithContent(Stream.Null, 100));

            context.Transport.Capabilities.Returns(TransportCapabilities.RequestStreaming);

            context.Parameters.Returns(RequestParameters.Empty.WithStrategy(new ParallelRequestStrategy(1)));

            ShouldPassChecks();
        }

        private static Request CreateCorrectRequest()
        {
            return Request.Get("foo/bar");
        }

        private static Request CreateIncorrectRequest()
        {
            return new Request(RequestMethods.Head, new Uri("foo/bar", UriKind.Relative), new Content(new byte[] {1, 2, 3}));
        }

        private void ShouldPassChecks()
        {
            var task = Task.FromResult(ClusterResult.UnexpectedException(context.Request));

            module.ExecuteAsync(context, _ => task).Should().BeSameAs(task);
        }

        private void ShouldFailChecks()
        {
            module.ExecuteAsync(context, _ => null).Result.Status.Should().Be(ClusterResultStatus.IncorrectArguments);
        }
    }
}