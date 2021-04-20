using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
// ReSharper disable once RedundantUsingDirective 
using Vostok.Clusterclient.Core.Misc; // KeyValuePairExtensions

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class StatisticsHistory_Tests
    {
        private StatisticsHistory statisticsHistory;

        [SetUp]
        public void SetUp()
        {
            statisticsHistory = new StatisticsHistory(1.Hours());
        }

        [Test]
        public void Get_should_return_null_statistic() =>
            statisticsHistory.Get().Should().BeNull();

        [Test]
        public void Update_should_add_new_statistic()
        {
            var clusterStatistic = new AggregatedStatistic(3, 15, DateTime.UtcNow);
            var replicasStatistic = new Dictionary<Uri, AggregatedStatistic>()
            {
                [new Uri("http://r1")] = new AggregatedStatistic(7, 2.5, DateTime.UtcNow),
                [new Uri("http://r2")] = new AggregatedStatistic(8, 9, DateTime.UtcNow),
                [new Uri("http://r3")] = new AggregatedStatistic(0.5, 0.35, DateTime.UtcNow),
            };

            statisticsHistory.Update(new AggregatedClusterStatistic(clusterStatistic, replicasStatistic));

            var clusterStats = statisticsHistory.Get();
            clusterStats.Cluster.Should().Be(clusterStatistic);
            clusterStats.Replicas.Count.Should().Be(3);
            foreach (var (replica, statistic) in clusterStats.Replicas)
                statistic.Should().Be(replicasStatistic[replica]);
        }

        [Test]
        public void Update_should_correct_update_current_statistic()
        {
            var r1 = new Uri("http://r1");
            var r2 = new Uri("http://r2");
            var r3 = new Uri("http://r3");
            var clusterStatistic = new AggregatedStatistic(3, 15, DateTime.UtcNow);
            var replicasStatistic = new Dictionary<Uri, AggregatedStatistic>()
            {
                [r1] = new AggregatedStatistic(7, 2.5, DateTime.UtcNow),
                [r2] = new AggregatedStatistic(8, 9, DateTime.UtcNow),
                [r3] = new AggregatedStatistic(0.5, 0.35, DateTime.UtcNow),
            };

            statisticsHistory.Update(new AggregatedClusterStatistic(clusterStatistic, replicasStatistic));

            var clusterStats = statisticsHistory.Get();
            clusterStats.Cluster.Should().Be(clusterStatistic);
            foreach (var (replica, statistic) in clusterStats.Replicas)
                statistic.Should().Be(replicasStatistic[replica]);

            var newCluster = new AggregatedStatistic(1, 1, DateTime.UtcNow);
            var newReplicas = new Dictionary<Uri, AggregatedStatistic>()
            {
                [new Uri("http://r3")] = new AggregatedStatistic(9.5, 1.35, DateTime.UtcNow),
            };

            statisticsHistory.Update(new AggregatedClusterStatistic(newCluster, newReplicas));
            
            clusterStats = statisticsHistory.Get();
            clusterStats.Cluster.Should().Be(newCluster);
            clusterStats.Replicas[r1].Should().Be(replicasStatistic[r1]);
            clusterStats.Replicas[r2].Should().Be(replicasStatistic[r2]);
            clusterStats.Replicas[r3].Should().Be(newReplicas[r3]);
        }

        [Test]
        public void Update_should_delete_obsolete_statistics()
        {
            statisticsHistory = new StatisticsHistory(1.Minutes());

            var r1 = new Uri("http://r1");
            var r2 = new Uri("http://r2");
            var r3 = new Uri("http://r3");
            var clusterStatistic = new AggregatedStatistic(3, 15, DateTime.UtcNow);
            var replicasStatistic = new Dictionary<Uri, AggregatedStatistic>()
            {
                [r1] = new AggregatedStatistic(7, 2.5, DateTime.UtcNow - 20.Seconds()),
                [r2] = new AggregatedStatistic(8, 9, DateTime.UtcNow - 40.Seconds()),
                [r3] = new AggregatedStatistic(0.5, 0.35, DateTime.UtcNow - 2.Minutes()),
            };

            statisticsHistory.Update(new AggregatedClusterStatistic(clusterStatistic, replicasStatistic));
            
            var clusterStats = statisticsHistory.Get();
            clusterStats.Cluster.Should().Be(clusterStatistic);
            clusterStats.Replicas.Count.Should().Be(3);
            foreach (var (replica, statistic) in clusterStats.Replicas)
                statistic.Should().Be(replicasStatistic[replica]);

            var newReplicasStats = new Dictionary<Uri, AggregatedStatistic>()
            {
                [r2] = new AggregatedStatistic(5, 10, DateTime.UtcNow)
            };
            statisticsHistory.Update(new AggregatedClusterStatistic(clusterStatistic, newReplicasStats));
            
            clusterStats = statisticsHistory.Get();
            clusterStats.Cluster.Should().Be(clusterStatistic);
            clusterStats.Replicas.Count.Should().Be(2);
            clusterStats.Replicas[r1].Should().Be(replicasStatistic[r1]);
            clusterStats.Replicas[r2].Should().Be(newReplicasStats[r2]);
        }
    }
}