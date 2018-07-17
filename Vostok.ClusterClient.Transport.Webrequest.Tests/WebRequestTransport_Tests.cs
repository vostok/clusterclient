using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Transport;
using Vostok.Logging.ConsoleLog;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests
{
    [TestFixture]
    internal class WebRequestTransport_Tests
    {
        private WebRequestTransport transport;

        [SetUp]
        public void TestSetup()
        {
            transport = new WebRequestTransport(new ConsoleLog());
        }

        [Test]
        public void Should_advertise_request_streaming_capability()
        {
            transport.Supports(TransportCapabilities.RequestStreaming).Should().BeTrue();
        }

        [Test]
        public void Should_advertise_response_streaming_capability()
        {
            transport.Supports(TransportCapabilities.ResponseStreaming).Should().BeTrue();
        }
    }
}