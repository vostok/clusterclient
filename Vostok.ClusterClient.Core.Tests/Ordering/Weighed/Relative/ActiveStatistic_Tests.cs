using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class ActiveStatistic_Tests
    {
        private ActiveStatistic activeStatistic;
        private const int PenaltyMultiplier = 50;

        [SetUp]
        public void SetUp()
        {
            activeStatistic = new ActiveStatistic(1.Seconds(), PenaltyMultiplier);
        }

        [Test]
        public void Should_correct_calculate_penalty()
        {
            var timestamp = DateTime.UtcNow;
            var replica = new Uri("http://replica");
            activeStatistic.Report(Accepted(replica, 125));
            activeStatistic.Report(Accepted(replica, 1325));
            activeStatistic.Report(Accepted(replica, 525));
            var statistic = activeStatistic.ObserveCluster(timestamp, 12, null);

            var penalty = activeStatistic.CalculatePenalty();

            penalty.Should().Be(statistic.Mean + statistic.StdDev * PenaltyMultiplier);
        }

        [Test]
        public void Report_should_add_result_to_replica_statistic()
        {
            var timestamp = DateTime.UtcNow;
            var replica = new Uri("http://replica");

            activeStatistic.Report(Accepted(replica, 125));
            activeStatistic.Report(Accepted(replica, 1325));
            var replicas = activeStatistic.ObserveReplicas(timestamp, 15, uri => null).ToArray();

            replicas.Length.Should().Be(1);
            replicas[0].Replica.Should().Be(replica);
            replicas[0].Statistic.Mean.Should().NotBeApproximately(0, 0.001);
            replicas[0].Statistic.StdDev.Should().NotBeApproximately(0, 0.001);
            replicas[0].Statistic.Timestamp.Should().Be(timestamp);
        }

        [Test]
        public void Report_should_add_result_to_cluster_statistic()
        {
            var timestamp = DateTime.UtcNow;
            var replica = new Uri("http://replica");

            activeStatistic.Report(Accepted(replica, 125));
            activeStatistic.Report(Accepted(replica, 1325));
            var statistic = activeStatistic.ObserveCluster(timestamp, 12, null);

            statistic.Mean.Should().NotBeApproximately(0, 0.001);
            statistic.StdDev.Should().NotBeApproximately(0, 0.001);
            statistic.Timestamp.Should().Be(timestamp);
        }

        private static ReplicaResult Accepted(Uri replica, int time) =>
            new ReplicaResult(replica, new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
    }
}