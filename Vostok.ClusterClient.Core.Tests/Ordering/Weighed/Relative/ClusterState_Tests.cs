using System;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class ClusterState_Tests
    {
        private ClusterState clusterState;

        [SetUp]
        public void SetUp()
        {
            clusterState = new ClusterState(new RelativeWeightSettings());
        }

        [Test]
        public void Exchange_should_swap_to_new_statistic()
        {
            var previousUpdateTime = clusterState.LastUpdateTimestamp;
            var previousActiveStat = clusterState.CurrentStatistic;
            
            Thread.Sleep(50);
            var _ = clusterState.ExchangeStatistic(DateTime.UtcNow);

            clusterState.LastUpdateTimestamp.Should().BeAfter(previousUpdateTime);
            clusterState.CurrentStatistic.Should().NotBeSameAs(previousActiveStat);
        }

        [Test]
        public void Exchange_should_return_correct_snapshot()
        {
            clusterState.CurrentStatistic.Report(Accepted("http://r1", 100));
            clusterState.CurrentStatistic.Report(Accepted("http://r2", 10));
            clusterState.CurrentStatistic.Report(Accepted("http://r3", 500));

            var snapshot = clusterState.ExchangeStatistic(DateTime.UtcNow);

            snapshot.Cluster.IsZero().Should().BeFalse();
            snapshot.Replicas.Count.Should().Be(3);
            foreach (var snapshotReplica in snapshot.Replicas)
                snapshotReplica.Value.IsZero().Should().BeFalse();
        }

        private static ReplicaResult Accepted(string replica, int time) =>
            new ReplicaResult(new Uri(replica), new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
    }
}