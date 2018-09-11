using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Criteria;

namespace Vostok.ClusterClient.Core.Tests.Criteria
{
    [TestFixture]
    internal class RejectRedirectionsCriterion_Tests
    {
        private RejectRedirectionsCriterion criterion;

        [SetUp]
        public void TestSetup()
        {
            criterion = new RejectRedirectionsCriterion();
        }

        [TestCase(ResponseCode.MultipleChoices)]
        [TestCase(ResponseCode.MovedPermanently)]
        [TestCase(ResponseCode.Found)]
        [TestCase(ResponseCode.SeeOther)]
        [TestCase(ResponseCode.UseProxy)]
        [TestCase(ResponseCode.TemporaryRedirect)]
        public void Should_reject_given_response_code(ResponseCode code)
        {
            criterion.Decide(new Response(code)).Should().Be(ResponseVerdict.Reject);
        }

        [Test]
        public void Should_know_nothing_about_codes_which_are_not_redirections()
        {
            var codes = Enum.GetValues(typeof (ResponseCode)).Cast<ResponseCode>().Where(code => !code.IsRedirection());

            foreach (var code in codes)
            {
                criterion.Decide(new Response(code)).Should().Be(ResponseVerdict.DontKnow);
            }
        }

        [Test]
        public void Should_know_nothing_about_not_modified_code()
        {
            criterion.Decide(new Response(ResponseCode.NotModified)).Should().Be(ResponseVerdict.DontKnow);
        }
    }
}
