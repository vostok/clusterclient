using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class RawClusterStatistic_Tests
    {
        private RawClusterStatistic rawClusterStatistic;
        private const int PenaltyMultiplier = 50;

        [SetUp]
        public void SetUp()
        {
            rawClusterStatistic = new RawClusterStatistic(1.Seconds(), PenaltyMultiplier);
        }

        [Test]
        public void Should_correct_calculate_penalty()
        {
            var replica = new Uri("http://replica");
            rawClusterStatistic.Report(Accepted(replica, 125));
            rawClusterStatistic.Report(Accepted(replica, 1325));
            rawClusterStatistic.Report(Accepted(replica, 525));
            
            var penalty = rawClusterStatistic.CalculatePenalty();

            penalty.Should().BeApproximately(25602.715, 0.001);
        }

        [Test]
        public void Report_should_add_result_to_replica_statistic()
        {
            var timestamp = DateTime.UtcNow;
            var replica = new Uri("http://replica");

            rawClusterStatistic.Report(Accepted(replica, 125));
            rawClusterStatistic.Report(Accepted(replica, 1325));
            var snapshot = rawClusterStatistic.GetPenalizedAndSmoothedStatistic(timestamp, null);

            snapshot.Replicas.Count.Should().Be(1);
            snapshot.Replicas[replica].Mean.Should().NotBeApproximately(0, 0.001);
            snapshot.Replicas[replica].StdDev.Should().NotBeApproximately(0, 0.001);
            snapshot.Replicas[replica].Timestamp.Should().Be(timestamp);
        }

        [Test]
        public void Report_should_add_result_to_cluster_statistic()
        {
            var timestamp = DateTime.UtcNow;
            var replica = new Uri("http://replica");

            rawClusterStatistic.Report(Accepted(replica, 125));
            rawClusterStatistic.Report(Accepted(replica, 1325));
            var snapshot = rawClusterStatistic.GetPenalizedAndSmoothedStatistic(timestamp, null);

            snapshot.Cluster.Mean.Should().NotBeApproximately(0, 0.001);
            snapshot.Cluster.StdDev.Should().NotBeApproximately(0, 0.001);
            snapshot.Cluster.Timestamp.Should().Be(timestamp);
        }

        private static ReplicaResult Accepted(Uri replica, int time) =>
            new ReplicaResult(replica, new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
    }
}