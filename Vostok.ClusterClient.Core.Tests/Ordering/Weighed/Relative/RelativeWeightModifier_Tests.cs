using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
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
        private IRelativeWeightCalculator relativeWeightCalculator;
        private IWeightsNormalizer weightsNormalizer;
        private IReplicaStorageProvider replicaStorageProvider;
        private IStatisticHistory statisticHistory;
        private RelativeWeightModifier relativeWeightModifier;
        
        [SetUp]
        public void SetUp()
        {
            statisticHistory = Substitute.For<IStatisticHistory>();
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
            clusterState = new ClusterState(settings, () =>
            {
                var activeStatistic = Substitute.For<IActiveStatistic>();
                activeStatistic.CalculateClusterStatistic(Arg.Any<DateTime>(), Arg.Any<double>(), Arg.Any<ClusterStatistic>())
                    .Returns(new ClusterStatistic(new Statistic(), new Dictionary<Uri, Statistic>()));
                return activeStatistic;
            }, statisticHistory, Substitute.For<IWeights>());
            replicaStorageProvider = Substitute.For<IReplicaStorageProvider>();
            relativeWeightCalculator = Substitute.For<IRelativeWeightCalculator>();
            weightsNormalizer = Substitute.For<IWeightsNormalizer>();
            replicaStorageProvider.ObtainGlobalValue(Arg.Any<string>(), Arg.Any<Func<ClusterState>>())
                .Returns(info => clusterState);
            relativeWeightModifier = new RelativeWeightModifier(settings, "srv", "env", relativeWeightCalculator, weightsNormalizer, 0, 1, 1);
        }

        [Test]
        public void Learn_should_report_to_active_statistic()
        {
            var replica = new Uri("http://r1");
            var replicaResult = Accepted(replica.OriginalString, 500);
            
            relativeWeightModifier.Learn(replicaResult, replicaStorageProvider);

            clusterState.CurrentStatistic.Received(1).Report(replicaResult);
        }

        [TestCase(-20, 0)]
        [TestCase(null, 2)]
        [TestCase(1100, 15)]
        public void Modify_should_enforce_global_weights_limits(double? internalWeight, double expected)
        {
            const int min = 0;
            const int initial = 2;
            const int max = 15;
            var replica1 = new Uri("http://r1");

            settings.WeightUpdatePeriod = 15.Seconds();
            clusterState.Weights.Get(replica1).Returns(info => internalWeight.HasValue ? new Weight(internalWeight.Value, DateTime.UtcNow): (Weight?) null);

            relativeWeightModifier = new RelativeWeightModifier(settings, "srv", "env", min, initial, max);

            var actualWeight = 1d;
            relativeWeightModifier.Modify(replica1, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref actualWeight);

            actualWeight.Should().Be(expected);
        }

        [Test]
        public void Modify_should_set_is_updating_flag_while_updating_weights()
        {
            var replica = new Uri("http://r1");
            var _ = 0d;
            var weightsUpdateTime = 250.Milliseconds();
            settings.WeightUpdatePeriod = 50.Milliseconds();
            clusterState.CurrentStatistic.CalculateClusterStatistic(Arg.Any<DateTime>(), Arg.Any<double>(), Arg.Any<ClusterStatistic>())
                .Returns(info => new ClusterStatistic(new Statistic(), new Dictionary<Uri, Statistic>()))
                .AndDoes(info => Thread.Sleep(weightsUpdateTime));
            
            clusterState.IsUpdatingNow.Value.Should().BeFalse();
            
            Thread.Sleep(settings.WeightUpdatePeriod);
            Task.Run(() => relativeWeightModifier.Modify(replica, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _));
            Thread.Sleep(100);

            clusterState.IsUpdatingNow.Value.Should().BeTrue();

            Thread.Sleep(weightsUpdateTime);
            clusterState.IsUpdatingNow.Value.Should().BeFalse();
        }

        [Test]
        public void Modify_should_not_update_weights_if_cluster_state_is_updating_now()
        {
            var replica = new Uri("http://r1");
            var _ = 0d;
            settings.WeightUpdatePeriod = 50.Milliseconds();
            clusterState.IsUpdatingNow.Value = true;

            Thread.Sleep(settings.WeightUpdatePeriod);
            (DateTime.UtcNow - clusterState.LastUpdateTimestamp).Should().BeGreaterThan(settings.WeightUpdatePeriod);
            relativeWeightModifier.Modify(replica, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _);

            relativeWeightCalculator.DidNotReceiveWithAnyArgs()
                .Calculate(Arg.Any<Statistic>(), Arg.Any<Statistic>(), Arg.Any<Weight>());
            clusterState.CurrentStatistic.DidNotReceiveWithAnyArgs()
                .CalculateClusterStatistic(Arg.Any<DateTime>(), Arg.Any<double>(), Arg.Any<ClusterStatistic>());
        }

        [Test]
        public void Modify_should_update_weights_after_time_period()
        {
            var replica = new Uri("http://r1");
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
        public void Modify_should_calculate_weights_for_each_replica_in_cluster_statistic()
        {
            var _ = 0d;
            var timestamp = DateTime.UtcNow;
            settings.WeightUpdatePeriod = 10.Milliseconds();
            var r1 = (new Uri("http://r1"), new Statistic(12, 150, timestamp), new Weight(0.5, timestamp));
            var r2 = (new Uri("http://r2"), new Statistic(15, 120, timestamp), new Weight(0.7, timestamp));
            var r3 = (new Uri("http://r3"), new Statistic(15, 110, timestamp), new Weight(0.1, timestamp));
            var clusterStatistic = new ClusterStatistic(new Statistic(13, 130, timestamp), new Dictionary<Uri, Statistic>()
            {
                [r1.Item1] = r1.Item2,
                [r2.Item1] = r2.Item2,
                [r3.Item1] = r3.Item2,
            });
            clusterState.CurrentStatistic.CalculateClusterStatistic(Arg.Any<DateTime>(), Arg.Any<double>(), Arg.Any<ClusterStatistic>())
                .Returns(clusterStatistic);
            relativeWeightCalculator.Calculate(clusterStatistic.Cluster, r1.Item2, Arg.Any<Weight>()).Returns(r1.Item3);
            relativeWeightCalculator.Calculate(clusterStatistic.Cluster, r2.Item2, Arg.Any<Weight>()).Returns(r2.Item3);
            relativeWeightCalculator.Calculate(clusterStatistic.Cluster, r3.Item2, Arg.Any<Weight>()).Returns(r3.Item3);
            
            Thread.Sleep(settings.WeightUpdatePeriod);
            relativeWeightModifier.Modify(r1.Item1, new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref _);

            clusterState.Weights.Received(1).Update(Arg.Any<IReadOnlyDictionary<Uri, Weight>>());
            weightsNormalizer.Received(1).Normalize(Arg.Any<Dictionary<Uri, Weight>>(), r2.Item3.Value);
            relativeWeightCalculator.Received(1).Calculate(clusterStatistic.Cluster, clusterStatistic.Replicas[r1.Item1], Arg.Any<Weight>());
            relativeWeightCalculator.Received(1).Calculate(clusterStatistic.Cluster, clusterStatistic.Replicas[r2.Item1], Arg.Any<Weight>());
            relativeWeightCalculator.Received(1).Calculate(clusterStatistic.Cluster, clusterStatistic.Replicas[r3.Item1], Arg.Any<Weight>());
        }

        private static ReplicaResult Accepted(string replica,int time) =>
            new ReplicaResult(new Uri(replica), new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
    }
}