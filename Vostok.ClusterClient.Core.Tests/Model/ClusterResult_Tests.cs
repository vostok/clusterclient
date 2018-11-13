using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Tests.Model
{
    [TestFixture]
    internal class ClusterResult_Tests
    {
        private Request request;

        [SetUp]
        public void TestSetup()
        {
            request = Request.Get("foo/bar");
        }

        [Test]
        public void Throttled_factory_method_should_return_correct_result()
        {
            var result = ClusterResult.Throttled(request);

            result.Status.Should().Be(ClusterResultStatus.Throttled);
            result.Request.Should().BeSameAs(request);
            result.ReplicaResults.Should().BeEmpty();
            result.Response.Code.Should().Be(ResponseCode.TooManyRequests);
        }
    }
}