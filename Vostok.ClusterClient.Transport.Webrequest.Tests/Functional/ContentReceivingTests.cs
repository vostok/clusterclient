using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Transport.Webrequest.Tests.Functional.Helpers;
using Vostok.Commons.Conversions;
using Vostok.Commons.Utilities;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional
{
    internal class ContentReceivingTests : TransportTestsBase
    {
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(500)]
        [TestCase(4096)]
        [TestCase(1024 * 1024)]
        [TestCase(4 * 1024 * 1024)]
        public void Should_be_able_to_receive_content_of_given_size(int size)
        {
            var content = ThreadSafeRandom.NextBytes(size.Bytes());

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = content.Length;
                    ctx.Response.OutputStream.Write(content, 0, content.Length);
                }))
            {
                var response = Send(Request.Put(server.Url));

                response.Content.ToArraySegment().Should().Equal(content);
            }
        }

        [Test]
        public void Should_read_response_body_greater_than_64k_with_non_successful_code()
        {
            var content = ThreadSafeRandom.NextBytes(100.Kilobytes());

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 409;
                    ctx.Response.OutputStream.Write(content, 0, content.Length);
                }))
            {
                var response = Send(Request.Put(server.Url));

                response.Code.Should().Be(ResponseCode.Conflict);
                response.Content.ToArraySegment().Should().Equal(content);
            }
        }

        [Test]
        public void Should_read_response_body_without_content_length()
        {
            var content = ThreadSafeRandom.NextBytes(500.Kilobytes());

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.OutputStream.Write(content, 0, content.Length);
                }))
            {
                var response = Send(Request.Put(server.Url));

                response.Content.ToArraySegment().Should().Equal(content);
            }
        }

        [Test]
        public void Should_return_http_517_when_response_body_size_is_larger_than_configured_limit_when_content_length_is_known()
        {
            Transport.Settings.MaxResponseBodySize = 1.Kilobytes();

            var content = ThreadSafeRandom.NextBytes(1.Kilobytes() + 1.Bytes());

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = content.Length;
                    ctx.Response.OutputStream.Write(content, 0, content.Length);
                }))
            {
                var response = Send(Request.Put(server.Url));

                response.Code.Should().Be(ResponseCode.InsufficientStorage);
                response.Content.Length.Should().Be(0);
            }
        }

        [Test]
        public void Should_return_http_517_when_response_body_size_is_larger_than_configured_limit_when_content_length_is_unknown()
        {
            Transport.Settings.MaxResponseBodySize = 1.Kilobytes();

            var content = ThreadSafeRandom.NextBytes(1.Kilobytes() + 1.Bytes());

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.OutputStream.Write(content, 0, content.Length);
                }))
            {
                var response = Send(Request.Put(server.Url));

                response.Code.Should().Be(ResponseCode.InsufficientStorage);
                response.Content.Length.Should().Be(0);
            }
        }

        [Test]
        public void Should_return_response_with_correct_content_length_when_buffer_factory_is_overriden()
        {
            Transport.Settings.BufferFactory = size => new byte[size * 2];

            var content = ThreadSafeRandom.NextBytes(1234);

            using (var server = TestServer.StartNew(
                ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentLength64 = content.Length;
                    ctx.Response.OutputStream.Write(content, 0, content.Length);
                }))
            {
                var response = Send(Request.Put(server.Url));

                response.Code.Should().Be(ResponseCode.Ok);
                response.Content.Buffer.Length.Should().Be(2468);
                response.Content.Offset.Should().Be(0);
                response.Content.Length.Should().Be(1234);
            }
        }
    }
}