using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Tests.Model
{
    public class RequestExtensions_Tests
    {
        private Request request;

        [SetUp]
        public void TestSetup()
        {
            request = new Request(RequestMethods.Post, new Uri("http://foo/bar?a=b"), Content.Empty, Headers.Empty);
        }

        [Test]
        public void Validation_procedures_should_pass_on_a_well_formed_http_request()
        {
            request.Validate().Should().BeEmpty();

            request.IsValidCustomizable(true).Should().BeTrue();
        }

        [Test]
        public void Validation_procedures_should_pass_on_a_well_formed_https_request()
        {
            request = new Request(request.Method, new Uri("https://foo/bar"));

            request.Validate().Should().BeEmpty();

            request.IsValidCustomizable(true).Should().BeTrue();
        }

        [Test]
        public void Validation_procedures_should_pass_if_request_has_unsupported_method_but_validateHttpMethod_are_false()
        {
            request = new Request("WHATEVER", request.Url);

            request.IsValidCustomizable(false).Should().BeTrue();
        }

        [Test]
        public void Validation_should_fail_if_request_has_unsupported_method()
        {
            request = new Request("WHATEVER", request.Url);

            request.IsValidCustomizable(true).Should().BeFalse();

            Console.Out.WriteLine(request.Validate().Single());
        }

        [Test]
        public void Validation_should_fail_if_request_has_an_url_with_non_http_scheme()
        {
            request = new Request(request.Method, new Uri("ftp://foo/bar"));

            request.IsValidCustomizable(true).Should().BeFalse();

            Console.Out.WriteLine(request.Validate().Single());
        }

        [Test]
        public void Validation_should_fail_when_supplying_request_body_buffer_with_get_method()
        {
            request = Request.Get(request.Url).WithContent(new Content(new byte[16]));

            request.IsValidCustomizable(true).Should().BeFalse();

            Console.Out.WriteLine(request.Validate().Single());
        }

        [Test]
        public void Validation_should_fail_when_supplying_request_body_stream_with_get_method()
        {
            request = Request.Get(request.Url).WithContent(new StreamContent(Stream.Null, 123));

            request.IsValidCustomizable(true).Should().BeFalse();

            Console.Out.WriteLine(request.Validate().Single());
        }

        [Test]
        public void Validation_should_fail_when_supplying_request_body_with_head_method()
        {
            request = Request.Head(request.Url).WithContent(new Content(new byte[16]));

            request.IsValidCustomizable(true).Should().BeFalse();

            Console.Out.WriteLine(request.Validate().Single());
        }
    }
}