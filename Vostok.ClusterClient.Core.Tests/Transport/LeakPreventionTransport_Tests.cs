using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Transport;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Tests.Transport
{
    [TestFixture]
    internal class LeakPreventionTransport_Tests
    {
        private Response response1;
        private Response response2;
        private Response response3;
        private Response response4;
        private Response response5;

        private ITransport baseTransport;
        private LeakPreventionTransport leakTransport;

        [SetUp]
        public void TestSetup()
        {
            response1 = Responses.Ok.WithStream(Substitute.For<Stream>());
            response2 = Responses.Ok.WithStream(Substitute.For<Stream>());
            response3 = Responses.Ok.WithStream(Substitute.For<Stream>());
            response4 = Responses.Ok.WithStream(Substitute.For<Stream>());
            response5 = Responses.Ok;

            baseTransport = Substitute.For<ITransport>();
            leakTransport = new LeakPreventionTransport(baseTransport);
        }

        [Test]
        public void Send_should_return_exactly_same_response_as_base_transport()
        {
            Send(response1).Should().BeSameAs(response1);
        }

        [Test]
        public void Send_should_not_dispose_responses_until_request_is_completed()
        {
            Send(response1);
            Send(response2);

            response1.Stream.DidNotReceive().Dispose();
            response2.Stream.DidNotReceive().Dispose();
        }

        [Test]
        public void Send_should_dispose_any_responses_coming_after_request_gets_completed()
        {
            Complete();

            Send(response1);
            Send(response2);

            response1.Stream.Received().Dispose();
            response2.Stream.Received().Dispose();
        }

        [Test]
        public void CompleteRequest_should_not_fail_if_no_requests_were_sent()
        {
            Complete(response1);
        }

        [Test]
        public void CompleteRequest_should_dispose_responses_whose_streams_are_not_in_final_result()
        {
            Send(response1);
            Send(response2);
            Send(response3);
            Send(response4);
            Send(response5);

            Complete(response1, response4);

            response1.Stream.DidNotReceive().Dispose();
            response4.Stream.DidNotReceive().Dispose();

            response2.Stream.Received().Dispose();
            response3.Stream.Received().Dispose();
        }

        private Response Send(Response response)
        {
            baseTransport.SendAsync(null, TimeSpan.Zero, CancellationToken.None).ReturnsForAnyArgs(response);

            return leakTransport.SendAsync(Request.Get(""), TimeSpan.MaxValue, CancellationToken.None).GetAwaiter().GetResult();
        }

        private void Complete(params Response[] responses)
        {
            responses = responses.Select(r => !r.HasStream ? r : r.WithHeader("a", "b")).ToArray();

            var replicaResults = responses.Select(r => new ReplicaResult(new Uri("http://replica"), r, ResponseVerdict.Accept, TimeSpan.Zero));

            var result = new ClusterResult(ClusterResultStatus.Success, replicaResults.ToList(), responses.FirstOrDefault(), Request.Get("")); 

            leakTransport.CompleteRequest(result);
        }
    }
}