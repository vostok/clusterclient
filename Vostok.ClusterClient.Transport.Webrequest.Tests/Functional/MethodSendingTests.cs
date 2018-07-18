using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Transport.Webrequest.Tests.Functional.Helpers;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional
{
    internal class MethodSendingTests : TransportTestsBase
    {
        [TestCaseSource(nameof(GetAllMethods))]
        public void Should_be_able_to_send_requests_with_given_method(string method)
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                Send(new Request(method, server.Url));

                server.LastRequest.Method.Should().Be(method);
            }
        }

        public static IEnumerable<object[]> GetAllMethods()
        {
            foreach (var method in RequestMethods.All)
            {
                yield return new object[] { method };
            }
        }
    }
}