using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class RelativeWeightModifier_Tests
    {
        private ClusterState clusterState;
        private RelativeWeightSettings settings;
        private IReplicaStorageProvider replicaStorageProvider;
        private RelativeWeightModifier relativeWeightModifier;
        
        [SetUp]
        public void SetUp()
        {
            settings = new RelativeWeightSettings()
            {
                WeightUpdatePeriod = 200.Milliseconds(),
                PenaltyMultiplier = 100,
                InitialWeight = 1,
                StatisticSmoothingConstant = 100.Milliseconds(),
                WeightsDownSmoothingConstant = 100.Milliseconds(),
                WeightsRaiseSmoothingConstant = 100.Milliseconds(),
                WeightsTTL = 5.Minutes(),
                MinWeight = 0.005,
                Sensitivity = 3
            };
            clusterState = new ClusterState(settings);
            replicaStorageProvider = Substitute.For<IReplicaStorageProvider>();
            replicaStorageProvider.ObtainGlobalValue(Arg.Any<string>(), Arg.Any<Func<ClusterState>>())
                .Returns(info => clusterState);
            relativeWeightModifier = new RelativeWeightModifier("srv", "env", settings);
        }

        [Test]
        public void Learn_should_report_to_replica_statistic()
        {
            var replica = new Uri("http://r1");

            clusterState.ActiveStatistic.ObserveReplicas(DateTime.UtcNow, 10, uri => null).Should().BeEmpty();

            relativeWeightModifier.Learn(Accepted(replica.OriginalString, 500), replicaStorageProvider);

            clusterState.ActiveStatistic.ObserveReplicas(DateTime.UtcNow, 10, uri => null).Should().NotBeEmpty();
        }

        [Test]
        public void Learn_should_report_to_cluster_statistic()
        {
            var replica = new Uri("http://r1");

            clusterState.ActiveStatistic.ObserveCluster(DateTime.UtcNow, 10, null).IsZero().Should().BeTrue();

            relativeWeightModifier.Learn(Accepted(replica.OriginalString, 500), replicaStorageProvider);

            clusterState.ActiveStatistic.ObserveCluster(DateTime.UtcNow, 10, null).IsZero().Should().BeFalse();
        }

        [Test]
        public void Modify_should_save_statistic_history()
        {
            var replica = new Uri("http://r1");
            var weight = 1d;
            clusterState.StatisticsHistory.Get(replica).Should().BeNull();
            clusterState.StatisticsHistory.GetCluster().Should().BeNull();
            relativeWeightModifier.Learn(Accepted(replica.OriginalString, 500), replicaStorageProvider);
            
            Thread.Sleep(settings.WeightUpdatePeriod);
            relativeWeightModifier.Modify(replica, new List<Uri>(){ replica}, replicaStorageProvider, Request.Get(replica), RequestParameters.Empty, ref weight);

            clusterState.StatisticsHistory.Get(replica).Should().NotBeNull();
            clusterState.StatisticsHistory.GetCluster().Should().NotBeNull();
        }

        [TestCaseSource(nameof(TestCaseSource))]
        public void Should_correct_modify_replicas_weights(ReplicaResult[] replicaResults, Dictionary<Uri, double> expectedWeights)
        {
            var replicas = replicaResults.Select(r => r.Replica).ToArray();

            foreach (var replicaResult in replicaResults)
            {
                relativeWeightModifier.Learn(replicaResult, replicaStorageProvider);
            }

            Thread.Sleep(settings.WeightUpdatePeriod);
            
            foreach (var replica in replicas)
            {
                var weight = 1d;
                
                relativeWeightModifier.Modify(replica, replicas, replicaStorageProvider, Request.Get(replica), RequestParameters.Empty, ref weight);
                
                weight.Should().BeApproximately(expectedWeights[replica], 0.001);
            }
        }

        private static readonly object[] TestCaseSource = {
            new object[]
            {
                new[]
                {
                    Accepted("http://r1", 150),
                    Accepted("http://r1", 250),
                    Accepted("http://r1", 135),
                    Accepted("http://r2", 50),
                    Rejected("http://r2", 25),
                    Accepted("http://r2", 100),
                    Rejected("http://r3", 5),
                    Rejected("http://r3", 10),
                    Rejected("http://r3", 15),
                },
                new Dictionary<Uri, double>()
                {
                    [new Uri("http://r1")] = 1,
                    [new Uri("http://r2")] = 0.318,
                    [new Uri("http://r3")] = 0.139,
                }
            }
        };
        private static ReplicaResult Accepted(string replica, int time) =>
            new ReplicaResult(new Uri(replica), new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
        private static ReplicaResult Rejected(string replica, int time) =>
            new ReplicaResult(new Uri(replica), new Response(ResponseCode.InternalServerError), ResponseVerdict.Reject, time.Milliseconds());
    }
}