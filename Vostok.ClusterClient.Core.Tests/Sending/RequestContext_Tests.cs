using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Tests.Helpers;
using Vostok.Logging.Console;

namespace Vostok.Clusterclient.Core.Tests.Sending
{
    [TestFixture]
    internal class RequestContext_Tests
    {
        private RequestContext context;

        [SetUp]
        public void TestSetup()
        {
            context = new RequestContext(
                Request.Get("foo/bar"),
                new RequestParameters(Strategy.Sequential2),
                Budget.Infinite,
                new ConsoleLog(),
                null,
                int.MaxValue);
        }

        [Test]
        public void SetReplicaResult_should_add_result_to_list_if_there_are_no_results_with_same_replica()
        {
            var result1 = CreateResult("replica1");
            var result2 = CreateResult("replica2");
            var result3 = CreateResult("replica3");

            context.SetReplicaResult(result1);
            context.SetReplicaResult(result2);
            context.SetReplicaResult(result3);

            context.FreezeReplicaResults().Should().Equal(result1, result2, result3);
        }

        [Test]
        public void SetReplicaResult_should_overwrite_existing_result_with_equal_replica_when_previous_code_is_unknown()
        {
            var result1 = CreateResult("replica1");
            var result2 = CreateResult("replica1");
            var result3 = CreateResult("replica1");
            var replicaUri = new Uri("http://replica1");

            context.SetUnknownResult(replicaUri);
            context.SetUnknownResult(replicaUri);
            context.SetUnknownResult(replicaUri);

            context.SetReplicaResult(result1);
            context.SetReplicaResult(result2);
            context.SetReplicaResult(result3);

            context.FreezeReplicaResults().Should().Equal(result1, result2, result3);
        }

        [Test]
        public void SetReplicaResult_should_not_overwrite_existing_result_with_equal_replica_when_previous_code_is_known()
        {
            var result1 = CreateResult("replica1");
            var result2 = CreateResult("replica1");
            var result3 = CreateResult("replica1");
            var replicaUri = new Uri("http://replica1");

            context.SetUnknownResult(replicaUri);
            context.SetReplicaResult(result1);

            context.SetUnknownResult(replicaUri);
            context.SetReplicaResult(result2);

            context.SetUnknownResult(replicaUri);
            context.SetReplicaResult(result3);

            context.FreezeReplicaResults().Should().Equal(result1, result2, result3);
        }

        [Test]
        public void SetReplicaResult_should_not_affect_already_frozen_results_list()
        {
            var result1 = CreateResult("replica1");
            var result2 = CreateResult("replica2");
            var result3 = CreateResult("replica3");

            context.SetReplicaResult(result1);
            context.SetReplicaResult(result2);

            var results = context.FreezeReplicaResults();

            context.SetReplicaResult(result3);

            results.Should().Equal(result1, result2);
        }

        [Test]
        public void ResetReplicaResults_should_initialize_new_results_list_and_allow_to_add_new_results()
        {
            var result1 = CreateResult("replica1");
            var result2 = CreateResult("replica2");
            var result3 = CreateResult("replica3");
            var result4 = CreateResult("replica4");
            var result5 = CreateResult("replica5");

            context.SetReplicaResult(result1);
            context.SetReplicaResult(result2);
            context.SetReplicaResult(result3);

            context.FreezeReplicaResults().Should().Equal(result1, result2, result3);
            context.ResetReplicaResults();

            context.SetReplicaResult(result4);
            context.SetReplicaResult(result5);

            context.FreezeReplicaResults().Should().Equal(result4, result5);
        }

        private static ReplicaResult CreateResult(string replica)
        {
            return new ReplicaResult(new Uri("http://" + replica), Responses.Timeout, ResponseVerdict.Accept, TimeSpan.Zero);
        }
    }
}