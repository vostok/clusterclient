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
                () => Substitute.For<IRawClusterStatistic>(), 
                statisticHistory);
        }

        [Test]
        public void Flush_should_swap_to_new_active_statistic()
        {
            var previousUpdateTime = clusterState.LastUpdateTimestamp;
            var previousActiveStat = clusterState.CurrentStatistic;
            
            Thread.Sleep(50);
            var _ = clusterState.FlushCurrentRawStatisticToHistory(DateTime.UtcNow);

            clusterState.LastUpdateTimestamp.Should().BeAfter(previousUpdateTime);
            clusterState.CurrentStatistic.Should().NotBeSameAs(previousActiveStat);
        }

        [Test]
        public void Flush_should_make_snapshot_according_to_statistic_history()
        {
            var timestamp = DateTime.UtcNow;
            var historyStatistic = new AggregatedClusterStatistic(new AggregatedStatistic(), new Dictionary<Uri, AggregatedStatistic>());

            clusterState.CurrentStatistic.GetPenalizedAndSmoothedStatistic(timestamp, Arg.Any<AggregatedClusterStatistic>())
                .Returns(info => new AggregatedClusterStatistic(new AggregatedStatistic(), new Dictionary<Uri, AggregatedStatistic>()));
            statisticHistory.Get().Returns(historyStatistic);

            var previousActiveStat = clusterState.CurrentStatistic;
            var _ = clusterState.FlushCurrentRawStatisticToHistory(timestamp);

            previousActiveStat.Received(1).GetPenalizedAndSmoothedStatistic(timestamp, historyStatistic);
            statisticHistory.Received(1).Get();
        }

        [Test]
        public void Flush_should_update_statistic_history()
        {
            var timestamp = DateTime.UtcNow;

            var clusterStatistic = clusterState.FlushCurrentRawStatisticToHistory(timestamp);

            statisticHistory.Received(1).Update(clusterStatistic);
        }
    }
}