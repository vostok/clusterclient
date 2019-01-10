using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core.Tests.Transport
{
    [TestFixture]
    internal class TimeoutHeaderTransport_Tests
    {
        private ITransport baseTransport;
        private TimeoutHeaderTransport timeoutTransport;

        [SetUp]
        public void TestSetup()
        {
            baseTransport = Substitute.For<ITransport>();
            timeoutTransport = new TimeoutHeaderTransport(baseTransport);
        }

        [TestCase(0.111, "0.111")]
        [TestCase(1, "1")]
        [TestCase(2, "2")]
        [TestCase(2.5, "2.5")]
        [TestCase(2.5551, "2.555", "custom-header")]
        public void Should_send_request_enriched_with_request_timeout_header_expressed_in_seconds(
            double seconds, string expected, string header = null)
        {
            var timeout = seconds.Seconds();
            var request = Request.Get("http://a/");

            var passed = false;

            baseTransport.WhenForAnyArgs(x => x.SendAsync(default, default, default, default))
                .Do(
                    c => passed = c.Arg<Request>().Headers[HeaderNames.RequestTimeout] == expected);

            timeoutTransport.SendAsync(request, default, timeout, default).GetAwaiter().GetResult();

            passed.Should().BeTrue();
        }
    }
}