using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Tests.Transport
{
    [TestFixture]
    internal class RequestTimeoutTransport_Tests
    {
        private ITransport baseTransport;
        private RequestTimeoutTransport timeoutTransport;

        [SetUp]
        public void TestSetup()
        {
            baseTransport = Substitute.For<ITransport>();
            timeoutTransport = new RequestTimeoutTransport(baseTransport);
        }

        [TestCase(0.111, "0.111")]
        [TestCase(1, "1")]
        [TestCase(2, "2")]
        [TestCase(2.5, "2.5")]
        public void Send_append_request_timeout_header(double seconds, string expected)
        {
            var timeout = seconds.Seconds();
            var request = Request.Get("http://a/");

            var passed = false;
            
            baseTransport.WhenForAnyArgs(x => x.SendAsync(default, default, default)).Do(
                c => passed = c.Arg<Request>().Headers[HeaderNames.RequestTimeout] == expected);
            
            timeoutTransport.SendAsync(request, timeout, default).GetAwaiter().GetResult();

            passed.Should().BeTrue();
        }
    }
}