using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Transport.Webrequest.Tests.Functional.Helpers;
using Vostok.Commons.Conversions;
using Vostok.Commons.Utilities;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional
{
    internal class ContentSendingTests : TransportTestsBase
    {
        [TestCase(1)]
        [TestCase(10)]
        [TestCase(500)]
        [TestCase(4096)]
        [TestCase(1024 * 1024)]
        [TestCase(4 * 1024 * 1024)]
        public void Should_be_able_to_send_buffered_content_of_given_size(int size)
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var content = ThreadSafeRandom.NextBytes(size.Bytes());

                var request = Request.Put(server.Url).WithContent(content);

                Send(request);

                server.LastRequest.Body.Should().Equal(content);
            }
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(500)]
        [TestCase(4096)]
        [TestCase(1024 * 1024)]
        [TestCase(4 * 1024 * 1024)]
        public void Should_be_able_to_send_stream_content_of_given_size_with_known_length(int size)
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var content = ThreadSafeRandom.NextBytes(size.Bytes());
                var guid = Guid.NewGuid().ToByteArray();

                var contentStream = new MemoryStream();

                contentStream.Write(content, 0, content.Length);
                contentStream.Write(guid, 0, guid.Length);
                contentStream.Position = 0;

                var request = Request.Put(server.Url).WithContent(contentStream, size);

                Send(request);

                server.LastRequest.Body.Should().Equal(content);
            }
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(500)]
        [TestCase(4096)]
        [TestCase(1024 * 1024)]
        [TestCase(4 * 1024 * 1024)]
        public void Should_be_able_to_send_stream_content_of_given_size_with_unknown_length(int size)
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var content = ThreadSafeRandom.NextBytes(size.Bytes());

                var contentStream = new MemoryStream(content, false);

                var request = Request.Put(server.Url).WithContent(contentStream);

                Send(request);

                server.LastRequest.Body.Should().Equal(content);
            }
        }

        [Test]
        public void Should_propagate_stream_reuse_exceptions()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Put(server.Url).WithContent(new AlreadyUsedStream());

                Action action = () => Send(request);

                var error = action.Should().ThrowExactly<StreamAlreadyUsedException>().Which;

                Console.Out.WriteLine(error);
            }
        }

        [Test]
        public void Should_return_stream_input_failure_code_when_user_provided_stream_throws_an_exception()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Put(server.Url).WithContent(new FailingStream());

                var response = Send(request);

                response.Code.Should().Be(ResponseCode.StreamInputFailure);
            }
        }

        #region EndlessStream

        private class EndlessZerosStream : Stream
        {
            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (count == 0)
                    return 0;

                return Math.Max(1, count - 1);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        #endregion

        #region AlreadyUsedStream

        private class AlreadyUsedStream : Stream
        {
            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new StreamAlreadyUsedException("Used up!");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        #endregion

        #region FailingStream

        private class FailingStream : Stream
        {
            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new IOException("This stream is wasted, try elsewhere.");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
        }

        #endregion
    }
}