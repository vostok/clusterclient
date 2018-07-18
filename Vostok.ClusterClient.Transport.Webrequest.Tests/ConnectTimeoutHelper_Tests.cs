using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.ConsoleLog;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests
{
    [TestFixture]
    internal class ConnectTimeoutHelper_Tests
    {
        private ILog log;

        [SetUp]
        public void TestSetup()
        {
            log = new ConsoleLog();
        }

        [Test]
        public void Should_be_able_to_build_checker_lambda()
        {
            ConnectTimeoutHelper.CanCheckSocket.Should().BeTrue();
        }
    }
}
