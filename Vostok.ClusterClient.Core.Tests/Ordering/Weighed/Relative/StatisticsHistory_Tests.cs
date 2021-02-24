using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class StatisticsHistory_Tests
    {
        private StatisticsHistory statisticsHistory;

        [SetUp]
        public void SetUp()
        {
            statisticsHistory = new StatisticsHistory();
        }

        [Test]
        public void Should_return_null_statistic_for_not_existing_replica()
        {
            var replica = new Uri("http://notexist");

            var statistic = statisticsHistory.GetForReplica(replica);

            statistic.HasValue.Should().BeFalse();
        }

        [Test]
        public void Should_return_null_global_statistic()
        {
            statisticsHistory.GetForCluster().HasValue.Should().BeFalse();
        }

        [Test]
        public void Update_should_add_new_statistic()
        {
            var clusterStatistic = new Statistic(3, 15, DateTime.UtcNow);
            var replicasStatistic = new Dictionary<Uri, Statistic>()
            {
                [new Uri("http://r1")] = new Statistic(7, 2.5, DateTime.UtcNow),
                [new Uri("http://r2")] = new Statistic(8, 9, DateTime.UtcNow),
                [new Uri("http://r3")] = new Statistic(0.5, 0.35, DateTime.UtcNow),
            };

            statisticsHistory.Update(new StatisticSnapshot(clusterStatistic, replicasStatistic));

            statisticsHistory.GetForCluster().Should().Be(clusterStatistic);
            foreach (var statistic in replicasStatistic)
                statisticsHistory.GetForReplica(statistic.Key).Should().Be(statistic.Value);
        }

        [Test]
        public void Update_should_correct_update_current_statistic()
        {
            var r1 = new Uri("http://r1");
            var r2 = new Uri("http://r2");
            var r3 = new Uri("http://r3");
            var clusterStatistic = new Statistic(3, 15, DateTime.UtcNow);
            var replicasStatistic = new Dictionary<Uri, Statistic>()
            {
                [r1] = new Statistic(7, 2.5, DateTime.UtcNow),
                [r2] = new Statistic(8, 9, DateTime.UtcNow),
                [r3] = new Statistic(0.5, 0.35, DateTime.UtcNow),
            };

            statisticsHistory.Update(new StatisticSnapshot(clusterStatistic, replicasStatistic));

            statisticsHistory.GetForCluster().Should().Be(clusterStatistic);
            foreach (var statistic in replicasStatistic)
                statisticsHistory.GetForReplica(statistic.Key).Should().Be(statistic.Value);

            var newCluster = new Statistic(1, 1, DateTime.UtcNow);
            var newReplicas = new Dictionary<Uri, Statistic>()
            {
                [new Uri("http://r3")] = new Statistic(9.5, 1.35, DateTime.UtcNow),
            };

            statisticsHistory.Update(new StatisticSnapshot(newCluster, newReplicas));

            statisticsHistory.GetForCluster().Should().Be(newCluster);
            statisticsHistory.GetForReplica(r1).Should().Be(replicasStatistic[r1]);
            statisticsHistory.GetForReplica(r2).Should().Be(replicasStatistic[r2]);
            statisticsHistory.GetForReplica(r3).Should().Be(newReplicas[r3]);
        }
    }
}