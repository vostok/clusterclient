using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Tests.Misc
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

        [TestCaseSource(nameof(GetAllMethods))]
        public void Validation_should_not_fail_when_supplying_request_body_with_any_method(string method)
        {
            request = new Request(method, request.Url).WithContent(new Content(new byte[16]));

            RequestValidator.IsValid(request).Should().BeTrue();
        }

        private static IEnumerable<object[]> GetAllMethods()
        {
            return HttpMethodValidationModule.All.Select(method => new object[] {method});
        }
    }
}