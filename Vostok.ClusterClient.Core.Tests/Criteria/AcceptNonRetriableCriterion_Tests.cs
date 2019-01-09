using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Tests.Criteria
{
    [TestFixture]
    internal class AcceptNonRetriableCriterion_Tests
    {
        private AcceptNonRetriableCriterion criterion;

        [SetUp]
        public void TestSetup()
        {
            criterion = new AcceptNonRetriableCriterion();
        }

        [Test]
        public void Should_accept_an_error_response_with_default_dont_retry_header()
        {
            var response = new Response(ResponseCode.ServiceUnavailable, headers: Headers.Empty.Set(HeaderNames.DontRetry, ""));

            criterion.Decide(response).Should().Be(ResponseVerdict.Accept);
        }

        [Test]
        public void Should_accept_an_error_response_with_custom_dont_retry_header()
        {
            criterion = new AcceptNonRetriableCriterion("custom-name");

            var response = new Response(ResponseCode.ServiceUnavailable, headers: Headers.Empty.Set("custom-name", ""));

            criterion.Decide(response).Should().Be(ResponseVerdict.Accept);
        }

        [Test]
        public void Should_know_nothing_about_an_error_response_without_dont_retry_header()
        {
            var response = new Response(ResponseCode.ServiceUnavailable, headers: Headers.Empty);

            criterion.Decide(response).Should().Be(ResponseVerdict.DontKnow);
        }
    }
}