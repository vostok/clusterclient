using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Core.Criteria;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Tests.Criteria
{
    [TestFixture]
    internal class RejectStreamingErrorsCriterion_Tests
    {
        private RejectStreamingErrorsCriterion criterion;

        [SetUp]
        public void TestSetup()
        {
            criterion = new RejectStreamingErrorsCriterion();
        }

        [TestCase(ResponseCode.StreamInputFailure)]
        [TestCase(ResponseCode.StreamReuseFailure)]
        public void Should_reject_given_response_code(ResponseCode code)
        {
            criterion.Decide(new Response(code)).Should().Be(ResponseVerdict.Reject);
        }

        [Test]
        public void Should_know_nothing_about_codes_which_are_not_unknown_errors()
        {
            var codes = Enum
                .GetValues(typeof(ResponseCode))
                .Cast<ResponseCode>()
                .Where(code =>
                    code != ResponseCode.StreamInputFailure &&
                    code != ResponseCode.StreamReuseFailure);

            foreach (var code in codes)
            {
                criterion.Decide(new Response(code)).Should().Be(ResponseVerdict.DontKnow);
            }
        }
    }
}