using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Logging.Abstractions;
using Vostok.Logging.ConsoleLog;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests
{
    [TestFixture]
    internal class WebRequestHeadersHacker_Tests
    {
        private ILog log;

        [SetUp]
        public void TestSetup()
        {
            log = new ConsoleLog();
        }

        [Test]
        public void Should_allow_to_set_values_directly_for_restricted_headers()
        {
            var request = WebRequest.CreateHttp("http://kontur.ru/");

            Action unsafeAssignment = () =>
            {
                request.Headers["Accept"] = "123";
                request.Headers["Range"] = "456";
                request.Headers["Content-Length"] = "789";
            };

            unsafeAssignment.Should().NotThrow();

            request.Headers["Accept"].Should().Be("123");
            request.Headers["Range"].Should().Be("456");
            request.Headers["Content-Length"].Should().Be("789");
        }
    }
}
