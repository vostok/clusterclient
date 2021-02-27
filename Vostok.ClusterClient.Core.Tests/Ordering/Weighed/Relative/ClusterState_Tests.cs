using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class ClusterState_Tests
    {
        private IStatisticHistory statisticHistory;
        private ClusterState clusterState;

        [SetUp]
        public void SetUp()
        {
            statisticHistory = Substitute.For<IStatisticHistory>();
            clusterState = new ClusterState(
                new RelativeWeightSettings(), 
                () => Substitute.For<IActiveStatistic>(), 
                statisticHistory);
        }

        [Test]
        public void Exchange_should_swap_to_new_active_statistic()
        {
            var previousUpdateTime = clusterState.LastUpdateTimestamp;
            var previousActiveStat = clusterState.CurrentStatistic;
            
            Thread.Sleep(50);
            var _ = clusterState.ExchangeStatistic(DateTime.UtcNow);

            clusterState.LastUpdateTimestamp.Should().BeAfter(previousUpdateTime);
            clusterState.CurrentStatistic.Should().NotBeSameAs(previousActiveStat);
        }

        [Test]
        public void Exchange_should_make_snapshot_according_to_statistic_history()
        {
            var timestamp = DateTime.UtcNow;

            clusterState.CurrentStatistic.ObserveReplicas(timestamp, Arg.Any<double>(), Arg.Any<Func<Uri, Statistic?>>())
                .Returns(new List<(Uri Replica, Statistic Statistic)>())
                .AndDoes(info => info.ArgAt<Func<Uri, Statistic?>>(2)(new Uri("http://r1")));

            var _ = clusterState.ExchangeStatistic(timestamp);

            statisticHistory.Received(1).GetForCluster();
            statisticHistory.Received(1).GetForReplica(Arg.Any<Uri>());
        }

        [Test]
        public void Exchange_should_make_snapshot_according_to_previous_statistic()
        {
            var timestamp = DateTime.UtcNow;
            var clusterStatistic = new Statistic(11, 15, timestamp);
            var penalty = 125;
            clusterState.CurrentStatistic.CalculatePenalty().Returns(penalty);
            statisticHistory.GetForCluster().Returns(clusterStatistic);
            clusterState.CurrentStatistic.ObserveReplicas(timestamp, Arg.Any<double>(), Arg.Any<Func<Uri, Statistic?>>())
                .Returns(new List<(Uri Replica, Statistic Statistic)>())
                .AndDoes(info => info.ArgAt<Func<Uri, Statistic?>>(2)(new Uri("http://r1")));

            var previousActiveStat = clusterState.CurrentStatistic;
            var _ = clusterState.ExchangeStatistic(timestamp);

            previousActiveStat.Received(1).ObserveCluster(timestamp, penalty, clusterStatistic);
            previousActiveStat.Received(1).ObserveReplicas(timestamp, penalty, Arg.Any<Func<Uri, Statistic?>>());
        }

        [Test]
        public void Exchange_should_update_statistic_history()
        {
            var timestamp = DateTime.UtcNow;

            var snapshot = clusterState.ExchangeStatistic(timestamp);

            statisticHistory.Received(1).Update(snapshot);
        }
    }
}