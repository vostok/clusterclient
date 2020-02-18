using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core.Tests.Transport
{
    [TestFixture]
    internal class ConnectionAttemptsTransport_Tests
    {
        private ITransport innerTransport;
        private Request request;
        private TimeSpan timeout;
        private TimeSpan? connectionTimeout;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("foo/bar");
            timeout = 5.Seconds();
            connectionTimeout = null;
            innerTransport = Substitute.For<ITransport>();
        }

        [Test]
        public void Should_retry_connection_timeout()
        {
            innerTransport
                .SendAsync(null, null, TimeSpan.Zero, CancellationToken.None)
                .ReturnsForAnyArgs(new Response(ResponseCode.ConnectFailure), new Response(ResponseCode.Ok));

            var transport = GetTransport(2);

            Send(transport).Code.Should().Be(ResponseCode.Ok);
            innerTransport.ReceivedWithAnyArgs(2).SendAsync(null, null, TimeSpan.Zero, CancellationToken.None);
        }

        [Test]
        public void Should_pass_ConnectionFailure_response_if_attempts_are_over()
        {
            innerTransport
                .SendAsync(null, null, TimeSpan.Zero, CancellationToken.None)
                .ReturnsForAnyArgs(
                    new Response(ResponseCode.ConnectFailure),
                    new Response(ResponseCode.ConnectFailure),
                    new Response(ResponseCode.ConnectFailure)
                );

            var transport = GetTransport(3);

            Send(transport).Code.Should().Be(ResponseCode.ConnectFailure);
            innerTransport.ReceivedWithAnyArgs(3).SendAsync(null, null, TimeSpan.Zero, CancellationToken.None);
        }

        private ConnectionAttemptsTransport GetTransport(int connectionAttempts = 1)
        {
            return new ConnectionAttemptsTransport(innerTransport, connectionAttempts);
        }

        private Response Send(ITransport transport, CancellationToken token = default)
        {
            return transport.SendAsync(request, connectionTimeout, timeout, token).GetAwaiter().GetResult();
        }
    }
}
