using System;
using System.IO;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Tests.Helpers;

namespace Vostok.Clusterclient.Core.Tests.Model
{
    [TestFixture]
    internal class Response_Tests
    {
        [Test]
        public void Headers_property_should_return_empty_headers_instead_of_null()
        {
            new Response(ResponseCode.Ok).Headers.Should().BeSameAs(Headers.Empty);
        }

        [Test]
        public void Content_property_should_return_empty_content_instead_of_null()
        {
            new Response(ResponseCode.Ok).Content.Should().BeSameAs(Content.Empty);
        }

        [Test]
        public void Content_property_should_return_empty_stream_instead_of_null()
        {
            new Response(ResponseCode.Ok).Stream.Should().BeSameAs(Stream.Null);
        }

        [TestCase(ResponseCode.Ok)]
        [TestCase(ResponseCode.Created)]
        [TestCase(ResponseCode.Accepted)]
        [TestCase(ResponseCode.NoContent)]
        [TestCase(ResponseCode.ResetContent)]
        [TestCase(ResponseCode.PartialContent)]
        [TestCase(ResponseCode.NonAuthoritativeInformation)]
        public void IsSuccessful_should_return_true_for_codes_from_2xx_family(ResponseCode code)
        {
            new Response(code).IsSuccessful.Should().BeTrue();
        }

        [TestCase(ResponseCode.Continue)]
        [TestCase(ResponseCode.MovedPermanently)]
        [TestCase(ResponseCode.NotFound)]
        [TestCase(ResponseCode.InternalServerError)]
        public void IsSuccessful_should_return_false_for_codes_from_families_other_than_2xx(ResponseCode code)
        {
            new Response(code).IsSuccessful.Should().BeFalse();
        }

        [Test]
        public void ToString_should_return_correct_representation_when_omitting_headers()
        {
            var response = new Response(ResponseCode.Ok, headers: Headers.Empty.Set("name", "value"));

            response.ToString(false).Should().Be("200 Ok");
        }

        [Test]
        public void ToString_should_return_correct_representation_when_printing_headers()
        {
            var response = new Response(ResponseCode.Ok, headers: Headers.Empty.Set("name", "value"));

            response.ToString(true).Should().Be("200 Ok" + Environment.NewLine + "name: value");
        }

        [Test]
        public void ToString_should_omit_headers_by_default()
        {
            var response = new Response(ResponseCode.Ok, headers: Headers.Empty.Set("name", "value"));

            response.ToString().Should().Be("200 Ok");
        }

        [Test]
        public void ToString_should_ignore_empty_headers()
        {
            var response = new Response(ResponseCode.Ok, headers: Headers.Empty);

            response.ToString(true).Should().Be("200 Ok");
        }

        [Test]
        public void ToString_should_ignore_null_headers()
        {
            var response = new Response(ResponseCode.Ok);

            response.ToString(true).Should().Be("200 Ok");
        }

        [TestCase(ResponseCode.InternalServerError)]
        [TestCase(ResponseCode.BadRequest)]
        [TestCase(ResponseCode.MovedPermanently)]
        [TestCase(ResponseCode.UnknownFailure)]
        public void EnsureSuccessStatusCode_should_throw_an_exception_for_non_2xx_code(ResponseCode code)
        {
            Action action = () => new Response(code).EnsureSuccessStatusCode();

            action.Should().Throw<ClusterClientException>().Which.ShouldBePrinted();
        }

        [TestCase(ResponseCode.Ok)]
        [TestCase(ResponseCode.Created)]
        [TestCase(ResponseCode.NoContent)]
        public void EnsureSuccessStatusCode_should_have_no_effect_for_2xx_code(ResponseCode code)
        {
            var response = new Response(code);

            response.EnsureSuccessStatusCode().Should().BeSameAs(response);
        }

        [Test]
        public void Dispose_should_not_fail_when_response_does_not_have_a_stream()
        {
            Responses.Ok.Dispose();
        }

        [Test]
        public void Dispose_should_close_content_stream()
        {
            var stream = Substitute.For<Stream>();

            Responses.Ok.WithStream(stream).Dispose();

            stream.Received().Dispose();
        }

        [Test]
        public void HasContent_should_return_false_when_there_is_no_content()
        {
            Responses.Ok.HasContent.Should().BeFalse();
        }

        [Test]
        public void HasHeaders_should_return_false_when_there_are_no_headers()
        {
            Responses.Ok.HasHeaders.Should().BeFalse();
        }

        [Test]
        public void HasStream_should_return_false_when_there_is_no_stream()
        {
            Responses.Ok.HasStream.Should().BeFalse();
        }

        [Test]
        public void HasContent_should_return_true_when_there_is_content()
        {
            Responses.Ok.WithContent("Hello!").HasContent.Should().BeTrue();
        }

        [Test]
        public void HasHeaders_should_return_true_when_there_are_some_headers()
        {
            Responses.Ok.WithHeader("key", "value").HasHeaders.Should().BeTrue();
        }

        [Test]
        public void HasStream_should_return_true_when_there_is_a_stream()
        {
            Responses.Ok.WithStream(new MemoryStream()).HasStream.Should().BeTrue();
        }

        [Test]
        public void RemoveHeader_should_remove_given_header()
        {
            var originalResponse = Responses.Ok
                .WithHeader("a", "b")
                .WithHeader("c", "d");

            var modifiedResponse = originalResponse.RemoveHeader("c");

            modifiedResponse.Headers["a"].Should().Be("b");
            modifiedResponse.Headers["c"].Should().BeNull();
        }

        [Test]
        public void RemoveHeader_should_preserve_code()
        {
            var originalResponse = Responses.Ok
                .WithHeader("a", "b")
                .WithHeader("c", "d");

            var modifiedResponse = originalResponse.RemoveHeader("c");

            modifiedResponse.Code.Should().Be(originalResponse.Code);
        }

        [Test]
        public void RemoveHeader_should_preserve_content()
        {
            var originalResponse = Responses.Ok
                .WithHeader("a", "b")
                .WithHeader("c", "d")
                .WithContent("Hello!");

            var modifiedResponse = originalResponse.RemoveHeader("c");

            modifiedResponse.Content.Should().BeSameAs(originalResponse.Content);
        }

        [Test]
        public void RemoveHeader_should_preserve_stream()
        {
            var originalResponse = Responses.Ok
                .WithHeader("a", "b")
                .WithHeader("c", "d")
                .WithStream(new MemoryStream());

            var modifiedResponse = originalResponse.RemoveHeader("c");

            modifiedResponse.Stream.Should().BeSameAs(originalResponse.Stream);
        }

        [Test]
        public void RemoveHeader_should_return_same_response_if_given_header_does_not_exist()
        {
            var originalResponse = Responses.Ok
                .WithHeader("a", "b")
                .WithHeader("c", "d");

            var modifiedResponse = originalResponse.RemoveHeader("e");

            modifiedResponse.Should().BeSameAs(originalResponse);
        }
    }
}