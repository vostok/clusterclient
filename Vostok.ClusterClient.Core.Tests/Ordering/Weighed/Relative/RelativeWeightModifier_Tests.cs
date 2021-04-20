using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;
using Vostok.Commons.Testing;

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
            clusterState = new ClusterState(settings,
                timeProvider: Substitute.For<ITimeProvider>(),
                rawClusterStatistic: Substitute.For<IRawClusterStatistic>(),
                statisticHistory: Substitute.For<IStatisticHistory>(), 
                relativeWeightCalculator: Substitute.For<IRelativeWeightCalculator>(),
                weights: Substitute.For<IWeights>());
            replicaStorageProvider = Substitute.For<IReplicaStorageProvider>();
            replicaStorageProvider.ObtainGlobalValue(Arg.Any<string>(), Arg.Any<Func<ClusterState>>())
                .Returns(info => clusterState);
            relativeWeightModifier = new RelativeWeightModifier(settings, "srv", "env", 0, 1);
        }

        [Test]
        public void Learn_should_report_to_current_row_statistic()
        {
            var replica = new Uri("http://r1");
            var replicaResult = Accepted(replica.OriginalString, 500);
            
            relativeWeightModifier.Learn(replicaResult, replicaStorageProvider);

            clusterState.CurrentStatistic.Received(1).Report(replicaResult);
        }

        [Test]
        public void Modify_should_not_update_weights_if_cluster_state_is_updating_now()
        {
            var replica = new Uri("http://r1");
            var _ = 0d;
            settings.WeightUpdatePeriod = 50.Milliseconds();
            clusterState.IsUpdatingNow.Value = true;

            (DateTime.UtcNow - clusterState.LastUpdateTimestamp).Should().BeGreaterThan(settings.WeightUpdatePeriod);
            relativeWeightModifier.Modify(replica, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _);

            clusterState.RelativeWeightCalculator.DidNotReceiveWithAnyArgs()
                .Calculate(Arg.Any<AggregatedStatistic>(), Arg.Any<AggregatedStatistic>(), Arg.Any<Weight>());
            clusterState.CurrentStatistic.DidNotReceiveWithAnyArgs()
                .GetPenalizedAndSmoothedStatistic(Arg.Any<DateTime>(), Arg.Any<AggregatedClusterStatistic>());
            clusterState.Weights.DidNotReceiveWithAnyArgs()
                .Update(Arg.Any<Dictionary<Uri, Weight>>());
        }

        [Test]
        public void Modify_should_update_weights_after_time_period()
        {
            var replica = new Uri("http://r1");
            clusterState.TimeProvider.GetCurrentTime().Returns(info => DateTime.UtcNow);
            clusterState.CurrentStatistic.GetPenalizedAndSmoothedStatistic(Arg.Any<DateTime>(), Arg.Any<AggregatedClusterStatistic>())
                .Returns(new AggregatedClusterStatistic(new AggregatedStatistic(), new Dictionary<Uri, AggregatedStatistic>()));
            settings.WeightUpdatePeriod = 100.Milliseconds();

            var lastUpdateTime = clusterState.LastUpdateTimestamp;

            Action assertion = () =>
            {
                var w = 0d;
                relativeWeightModifier.Modify(replica, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref w);
                clusterState.LastUpdateTimestamp.Should().BeAfter(lastUpdateTime);
                clusterState.Weights.Received(1).Update(Arg.Any<IReadOnlyDictionary<Uri, Weight>>());
            };
            assertion.ShouldPassIn(settings.WeightUpdatePeriod, 10.Milliseconds());
        }

        [Test]
        public void Modify_should_update_cluster_state_timestamp()
        {
            var _ = 0d;
            var timestamp = DateTime.UtcNow;
            clusterState.LastUpdateTimestamp = timestamp;
            clusterState.TimeProvider.GetCurrentTime().Returns(timestamp + settings.WeightUpdatePeriod + 1.Seconds());
            var clusterStatistic = new AggregatedClusterStatistic(new AggregatedStatistic(13, 130, timestamp), new Dictionary<Uri, AggregatedStatistic>());
            clusterState.CurrentStatistic.GetPenalizedAndSmoothedStatistic(Arg.Any<DateTime>(), Arg.Any<AggregatedClusterStatistic>())
                .Returns(clusterStatistic);

            relativeWeightModifier.Modify(new Uri("http://replica1"), new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _);

            clusterState.LastUpdateTimestamp.Should().Be(timestamp + settings.WeightUpdatePeriod + 1.Seconds());
        }

        [Test]
        public void Modify_should_update_statistics_history()
        {
            var _ = 0d;
            var timestamp = DateTime.UtcNow;
            clusterState.LastUpdateTimestamp = timestamp;
            clusterState.TimeProvider.GetCurrentTime().Returns(timestamp + settings.WeightUpdatePeriod + 1.Seconds());
            var clusterStatistic = new AggregatedClusterStatistic(new AggregatedStatistic(13, 130, timestamp), new Dictionary<Uri, AggregatedStatistic>());
            clusterState.CurrentStatistic.GetPenalizedAndSmoothedStatistic(Arg.Any<DateTime>(), Arg.Any<AggregatedClusterStatistic>())
                .Returns(clusterStatistic);

            relativeWeightModifier.Modify(new Uri("http://replica1"), new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _);

            clusterState.StatisticHistory.Received(1).Update(clusterStatistic);
        }

        [Test]
        public void Modify_should_calculate_weights_for_each_replica_in_cluster_statistic()
        {
            var _ = 0d;
            var timestamp = DateTime.UtcNow;
            clusterState.LastUpdateTimestamp = timestamp;
            clusterState.TimeProvider.GetCurrentTime().Returns(timestamp + settings.WeightUpdatePeriod + 1.Seconds());

            var r1 = (new Uri("http://r1"), new AggregatedStatistic(12, 150, timestamp), new Weight(0.5, timestamp));
            var r2 = (new Uri("http://r2"), new AggregatedStatistic(15, 120, timestamp), new Weight(0.7, timestamp));
            var r3 = (new Uri("http://r3"), new AggregatedStatistic(15, 110, timestamp), new Weight(0.1, timestamp));
            var clusterStatistic = new AggregatedClusterStatistic(new AggregatedStatistic(13, 130, timestamp), new Dictionary<Uri, AggregatedStatistic>()
            {
                [r1.Item1] = r1.Item2,
                [r2.Item1] = r2.Item2,
                [r3.Item1] = r3.Item2,
            });
            clusterState.CurrentStatistic.GetPenalizedAndSmoothedStatistic(Arg.Any<DateTime>(), Arg.Any<AggregatedClusterStatistic>())
                .Returns(clusterStatistic);
            clusterState.RelativeWeightCalculator.Calculate(clusterStatistic.Cluster, r1.Item2, Arg.Any<Weight>()).Returns(r1.Item3);
            clusterState.RelativeWeightCalculator.Calculate(clusterStatistic.Cluster, r2.Item2, Arg.Any<Weight>()).Returns(r2.Item3);
            clusterState.RelativeWeightCalculator.Calculate(clusterStatistic.Cluster, r3.Item2, Arg.Any<Weight>()).Returns(r3.Item3);
            
            relativeWeightModifier.Modify(r1.Item1, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _);

            clusterState.RelativeWeightCalculator.Received(1).Calculate(clusterStatistic.Cluster, clusterStatistic.Replicas[r1.Item1], Arg.Any<Weight>());
            clusterState.RelativeWeightCalculator.Received(1).Calculate(clusterStatistic.Cluster, clusterStatistic.Replicas[r2.Item1], Arg.Any<Weight>());
            clusterState.RelativeWeightCalculator.Received(1).Calculate(clusterStatistic.Cluster, clusterStatistic.Replicas[r3.Item1], Arg.Any<Weight>());
        }

        private static ReplicaResult Accepted(string replica,int time) =>
            new ReplicaResult(new Uri(replica), new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
    }
}