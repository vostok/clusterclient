using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Sending;
using Vostok.Logging.Console;

namespace Vostok.ClusterClient.Core.Tests.Sending
{
    [TestFixture]
    internal class RequestConverter_Tests
    {
        private RequestConverter converter;

        [SetUp]
        public void TestSetup()
        {
            converter = new RequestConverter(new ConsoleLog(), false);
        }

        [TestCase("foo/bar")]
        [TestCase("/foo/bar")]
        [TestCase("/")]
        [TestCase("")]
        public void Should_return_null_when_replica_url_is_not_absolute(string replicaUrl)
        {
            converter.TryConvertToAbsolute(Request.Get("request/path"), new Uri(replicaUrl, UriKind.Relative)).Should().BeNull();
        }

        [Test]
        public void Should_return_null_when_replica_url_contains_query_parameters()
        {
            converter.TryConvertToAbsolute(Request.Get("request/path"), new Uri("http://replica?a=b")).Should().BeNull();
        }

        [Test]
        public void Should_return_null_when_request_url_is_absolute()
        {
            converter.TryConvertToAbsolute(Request.Get("http://host/request/path"), new Uri("http://replica")).Should().BeNull();
        }

        [TestCase("http://replica:123/foo", "bar/baz", "http://replica:123/foo/bar/baz")]
        [TestCase("http://replica:123/foo", "bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123/foo/", "bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123/foo", "/bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123/foo/", "/bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123", "/bar/baz?k=v", "http://replica:123/bar/baz?k=v")]
        [TestCase("http://replica:123", "/?k=v", "http://replica:123/?k=v")]
        [TestCase("http://replica:123/", "/?k=v", "http://replica:123/?k=v")]
        [TestCase("http://replica:123", "?k=v", "http://replica:123/?k=v")]
        public void Should_return_request_with_correct_merged_url(string replicaUrl, string requestUrl, string expectedUrl)
        {
            var convertedRequest = converter.TryConvertToAbsolute(Request.Get(requestUrl), new Uri(replicaUrl));

            convertedRequest.Should().NotBeNull();

            convertedRequest.Url.IsAbsoluteUri.Should().BeTrue();

            convertedRequest.Url.OriginalString.Should().Be(expectedUrl);
        }

        [TestCase("http://replica:123/foo", "bar/baz", "http://replica:123/foo/bar/baz")]
        [TestCase("http://replica:123/foo", "bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123/foo/", "bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123/foo", "/bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123/foo/", "/bar/baz?k=v", "http://replica:123/foo/bar/baz?k=v")]
        [TestCase("http://replica:123", "/bar/baz?k=v", "http://replica:123/bar/baz?k=v")]
        [TestCase("http://replica:123", "/?k=v", "http://replica:123/?k=v")]
        [TestCase("http://replica:123/", "/?k=v", "http://replica:123/?k=v")]
        [TestCase("http://replica:123", "?k=v", "http://replica:123/?k=v")]
        [TestCase("http://replica:123", "foo/bar?k=v", "http://replica:123/foo/bar?k=v")]
        [TestCase("http://replica:123", "/foo/bar?k=v", "http://replica:123/foo/bar?k=v")]
        [TestCase("http://replica:123", "/foo/bar/?k=v", "http://replica:123/foo/bar/?k=v")]
        [TestCase("http://replica:123/drive/v1/", "/foo/bar?k=v", "http://replica:123/drive/v1/foo/bar?k=v")]
        [TestCase("http://replica:123/drive/v1/", "v1/foo/bar?k=v", "http://replica:123/drive/v1/foo/bar?k=v")]
        [TestCase("http://replica:123/drive/v1/", "drive/v1/foo/bar?k=v", "http://replica:123/drive/v1/foo/bar?k=v")]
        [TestCase("http://replica:123/drive/v1/", "/drive/v1/foo/bar?k=v", "http://replica:123/drive/v1/foo/bar?k=v")]
        [TestCase("http://replica:123/api/drive/v1/", "drive/foo/bar?k=v", "http://replica:123/api/drive/v1/drive/foo/bar?k=v")]
        [TestCase("http://replica:123/api-yuhapi/", "api-yuhapi/foo/bar?k=v", "http://replica:123/api-yuhapi/foo/bar?k=v")]
        [TestCase("http://replica:123/api-yuhapi/", "api_yuhapi/foo/bar?k=v", "http://replica:123/api-yuhapi/api_yuhapi/foo/bar?k=v")]
        [TestCase("http://replica:123/api-yuhapi/", "api-yuhapi", "http://replica:123/api-yuhapi/")]
        [TestCase("http://replica:123/api-yuhapi/", "api-yuhapi/", "http://replica:123/api-yuhapi/")]
        [TestCase("http://replica:123/api-yuhapi/", "api-yuhapi/foo", "http://replica:123/api-yuhapi/foo")]
        [TestCase("http://replica:123/api-yuhapi/", "api-yuhapi/foo/", "http://replica:123/api-yuhapi/foo/")]
        [TestCase("http://replica:123/api-yuhapi/", "", "http://replica:123/api-yuhapi/")]
        [TestCase("http://replica:123/api-yuhapi/", "/", "http://replica:123/api-yuhapi/")]
        [TestCase("http://replica:123/api-yuhapi", "api-yuhapi", "http://replica:123/api-yuhapi")]
        [TestCase("http://replica:123/api-yuhapi", "api-yuhapi/", "http://replica:123/api-yuhapi/")]
        [TestCase("http://replica:123/api-yuhapi", "api-yuhapi/foo", "http://replica:123/api-yuhapi/foo")]
        [TestCase("http://replica:123/api-yuhapi", "api-yuhapi/foo/", "http://replica:123/api-yuhapi/foo/")]
        [TestCase("http://replica:123/api-yuhapi", "", "http://replica:123/api-yuhapi")]
        [TestCase("http://replica:123/api-yuhapi", "/", "http://replica:123/api-yuhapi/")]
        public void Should_return_request_with_correct_merged_url_with_path_segments_deduplication(string replicaUrl, string requestUrl, string expectedUrl)
        {
            converter = new RequestConverter(new ConsoleLog(), true);

            var convertedRequest = converter.TryConvertToAbsolute(Request.Get(requestUrl), new Uri(replicaUrl));

            convertedRequest.Should().NotBeNull();

            convertedRequest.Url.IsAbsoluteUri.Should().BeTrue();

            convertedRequest.Url.OriginalString.Should().Be(expectedUrl);
        }
    }
}