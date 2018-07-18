using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Transport.Webrequest.Tests.Functional.Helpers;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional
{
    internal class HeaderSendingTests : TransportTestsBase
    {
        //after each semicolon must be space
        [TestCase(HeaderNames.Accept, "text/html")]
        [TestCase(HeaderNames.Accept, "audio/*; q=0.2, audio/basic")]
        [TestCase(HeaderNames.AcceptCharset, "utf-8")]
        [TestCase(HeaderNames.AcceptCharset, "iso-8859-5, unicode-1-1; q=0.8")]
        [TestCase(HeaderNames.AcceptEncoding, "*")]
        [TestCase(HeaderNames.AcceptEncoding, "gzip; q=1.0, identity; q=0.5, *; q=0.0")]    //all numbers must be float
        [TestCase(HeaderNames.AcceptLanguage, "da, en-gb; q=0.8, en; q=0.7")]
        [TestCase(HeaderNames.Authorization, "Basic YWxhZGRpbjpvcGVuc2VzYW1l")]
        [TestCase(HeaderNames.ContentEncoding, "identity")]
        [TestCase(HeaderNames.ContentEncoding, "gzip")]
        [TestCase(HeaderNames.ContentLanguage, "mi, en")]
        [TestCase(HeaderNames.ContentRange, "bytes 200-1000/67589")]
        [TestCase(HeaderNames.ContentType, "text/html; charset=utf-8")]
        [TestCase(HeaderNames.ContentType, "multipart/form-data; boundary=gc0p4Jq0M2Yt08jU534c0p")]
        [TestCase(HeaderNames.Date, "Tue, 15 Nov 1994 08:12:31 GMT")]
        [TestCase(HeaderNames.ETag, "\"bfc13a64729c4290ef5b2c2730249c88ca92d82d\"")]
        [TestCase(HeaderNames.Host, "vm-service")]
        [TestCase(HeaderNames.IfMatch, "\"bfc13a64729c4290ef5b2c2730249c88ca92d82d\"")]
        [TestCase(HeaderNames.IfNoneMatch, "\"bfc13a64729c4290ef5b2c2730249c88ca92d82d\"")]
        [TestCase(HeaderNames.IfModifiedSince, "Wed, 21 Oct 2015 07:28:00 GMT")]
        [TestCase(HeaderNames.LastModified, "Wed, 21 Oct 2015 07:28:00 GMT")]
        [TestCase(HeaderNames.Location, "http://server:545/file")]
        [TestCase(HeaderNames.Range, "bytes=200-1000")]
        [TestCase(HeaderNames.Referer, "whatever")]
        [TestCase(HeaderNames.TE, "trailers, deflate; q=0.5")]
        [TestCase(HeaderNames.Upgrade, "HTTP/2.0, SHTTP/1.3, IRC/6.9, RTA/x11")]
        [TestCase(HeaderNames.UserAgent, "Firefox")]
        [TestCase(HeaderNames.Via, "Stargate")]
        [TestCase(HeaderNames.XKonturRequestPriority, "Sheddable")]
        public void Should_correctly_transfer_given_header_to_server(string headerName, string headerValue)
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Post(server.Url).WithHeader(headerName, headerValue);

                Send(request);

                server.LastRequest.Headers[headerName].Should().Be(headerValue);
            }
        }

        [Test]
        public void Should_include_auxiliary_client_identity_header()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Get(server.Url);

                Send(request);

                server.LastRequest.Headers[HeaderNames.XKonturClientIdentity].Should().NotBeNull();
            }
        }

        [Test]
        public void Should_include_auxiliary_request_timeout_header()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Get(server.Url);

                Send(request);

                server.LastRequest.Headers[HeaderNames.XKonturRequestTimeout].Should().NotBeNull();
            }
        }

        [Test]
        public void Should_not_override_user_provided_auxiliary_client_identity_header()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Get(server.Url).WithHeader(HeaderNames.XKonturClientIdentity, "123");

                Send(request);

                server.LastRequest.Headers[HeaderNames.XKonturClientIdentity].Should().Be("123");
            }
        }

        [Test]
        public void Should_override_user_provided_auxiliary_request_timeout_header()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Get(server.Url).WithHeader(HeaderNames.XKonturRequestTimeout, "123");

                Send(request);

                server.LastRequest.Headers[HeaderNames.XKonturRequestTimeout].Should().NotBe("123");
            }
        }

        [Test]
        public void Should_ignore_transfer_encoding_header()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request.Get(server.Url).WithHeader(HeaderNames.TransferEncoding, "chunked");

                Send(request);

                server.LastRequest.Headers[HeaderNames.TransferEncoding].Should().BeNull();
            }
        }
    }
}