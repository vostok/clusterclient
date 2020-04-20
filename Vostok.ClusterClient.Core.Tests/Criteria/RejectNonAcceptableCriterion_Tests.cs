using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Criteria;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Tests.Criteria
{
    [TestFixture]
    internal class RejectNonAcceptableCriterion_Tests
    {
        private RejectNonAcceptableCriterion criterion;

        [SetUp]
        public void TestSetup()
        {
            criterion = new RejectNonAcceptableCriterion();
        }

        [Test]
        public void Should_accept_an_error_response_with_default_dont_accept_header()
        {
            var response = new Response(ResponseCode.Ok, headers: Headers.Empty.Set(HeaderNames.DontAccept, ""));

            criterion.Decide(response).Should().Be(ResponseVerdict.Reject);
        }

        [Test]
        public void Should_know_nothing_about_an_error_response_without_dont_accept_header()
        {
            var response = new Response(ResponseCode.Ok, headers: Headers.Empty);

            criterion.Decide(response).Should().Be(ResponseVerdict.DontKnow);
        }
    }
}
