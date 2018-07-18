using FluentAssertions;
using NUnit.Framework;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Transport.Webrequest.Tests.Functional.Helpers;

namespace Vostok.ClusterClient.Transport.Webrequest.Tests.Functional
{
    internal class QuerySendingTests : TransportTestsBase
    {
        [Test]
        public void Should_correctly_transfer_query_parameters_to_server()
        {
            using (var server = TestServer.StartNew(ctx => ctx.Response.StatusCode = 200))
            {
                var request = Request
                    .Get(server.Url)
                    .WithAdditionalQueryParameter("key1", "value1")
                    .WithAdditionalQueryParameter("key2", "value2")
                    .WithAdditionalQueryParameter("ключ3", "значение3");

                Send(request);

                server.LastRequest.Query["key1"].Should().Be("value1");
                server.LastRequest.Query["key2"].Should().Be("value2");
                server.LastRequest.Query["ключ3"].Should().Be("значение3");
            }
        }
    }
}