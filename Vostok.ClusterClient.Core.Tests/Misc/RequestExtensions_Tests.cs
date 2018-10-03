using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Misc;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Tests.Misc
{
    internal class RequestValidator_Tests
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
            RequestValidator.Validate(request).Should().BeEmpty();

            RequestValidator.IsValid(request).Should().BeTrue();
        }

        [Test]
        public void Validation_procedures_should_pass_on_a_well_formed_https_request()
        {
            request = new Request(request.Method, new Uri("https://foo/bar"));

            RequestValidator.Validate(request).Should().BeEmpty();

            RequestValidator.IsValid(request).Should().BeTrue();
        }

        [Test]
        public void Validation_should_fail_if_request_has_an_url_with_non_http_scheme()
        {
            request = new Request(request.Method, new Uri("ftp://foo/bar"));

            RequestValidator.IsValid(request).Should().BeFalse();

            Console.Out.WriteLine(RequestValidator.Validate(request).Single());
        }

        [Test]
        public void Validation_should_fail_when_supplying_request_body_buffer_with_get_method()
        {
            request = Request.Get(request.Url).WithContent(new Content(new byte[16]));

            RequestValidator.IsValid(request).Should().BeFalse();

            Console.Out.WriteLine(RequestValidator.Validate(request).Single());
        }

        [Test]
        public void Validation_should_fail_when_supplying_request_body_stream_with_get_method()
        {
            request = Request.Get(request.Url).WithContent(new StreamContent(Stream.Null, 123));

            RequestValidator.IsValid(request).Should().BeFalse();

            Console.Out.WriteLine(RequestValidator.Validate(request).Single());
        }

        [Test]
        public void Validation_should_fail_when_supplying_request_body_with_head_method()
        {
            request = Request.Head(request.Url).WithContent(new Content(new byte[16]));

            RequestValidator.IsValid(request).Should().BeFalse();

            Console.Out.WriteLine(RequestValidator.Validate(request).Single());
        }
    }
}