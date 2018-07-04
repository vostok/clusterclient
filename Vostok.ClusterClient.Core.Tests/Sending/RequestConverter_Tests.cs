using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Sending;
using Vostok.Logging.ConsoleLog;

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
            AssertionExtensions.Should((object)converter.TryConvertToAbsolute(Request.Get("request/path"), new Uri(replicaUrl, UriKind.Relative))).BeNull();
        }

        [Test]
        public void Should_return_null_when_replica_url_contains_query_parameters()
        {
            AssertionExtensions.Should((object)converter.TryConvertToAbsolute(Request.Get("request/path"), new Uri("http://replica?a=b"))).BeNull();
        }

        [Test]
        public void Should_return_null_when_request_url_is_absolute()
        {
            AssertionExtensions.Should((object)converter.TryConvertToAbsolute(Request.Get("http://host/request/path"), new Uri("http://replica"))).BeNull();
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

            AssertionExtensions.Should((object)convertedRequest).NotBeNull();

            AssertionExtensions.Should((bool)convertedRequest.Url.IsAbsoluteUri).BeTrue();

            AssertionExtensions.Should((string)convertedRequest.Url.OriginalString).Be(expectedUrl);
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
        [TestCase("http://replica:123/api-huyapi/", "api-huyapi/foo/bar?k=v", "http://replica:123/api-huyapi/foo/bar?k=v")]
        [TestCase("http://replica:123/api-huyapi/", "api_huyapi/foo/bar?k=v", "http://replica:123/api-huyapi/api_huyapi/foo/bar?k=v")]
        public void Should_return_request_with_correct_merged_url_with_path_segments_deduplication(string replicaUrl, string requestUrl, string expectedUrl)
        {
            converter = new RequestConverter(new ConsoleLog(), true);

            var convertedRequest = converter.TryConvertToAbsolute(Request.Get(requestUrl), new Uri(replicaUrl));

            AssertionExtensions.Should((object)convertedRequest).NotBeNull();

            AssertionExtensions.Should((bool)convertedRequest.Url.IsAbsoluteUri).BeTrue();

            AssertionExtensions.Should((string)convertedRequest.Url.OriginalString).Be(expectedUrl);
        }
    }
}
