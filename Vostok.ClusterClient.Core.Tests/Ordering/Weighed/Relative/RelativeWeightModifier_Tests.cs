using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces;
using Vostok.Clusterclient.Core.Tests.Helpers;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class RelativeWeightModifier_Tests
    {
        private ClusterState clusterState;
        private RelativeWeightSettings settings;
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
            clusterState = new ClusterState(settings, () => Substitute.For<IActiveStatistic>(), statisticHistory, Substitute.For<IWeights>());
            replicaStorageProvider = Substitute.For<IReplicaStorageProvider>();
            replicaStorageProvider.ObtainGlobalValue(Arg.Any<string>(), Arg.Any<Func<ClusterState>>())
                .Returns(info => clusterState);
            relativeWeightModifier = new RelativeWeightModifier("srv", "env", settings, null);
        }

        [Test]
        public void Learn_should_report_to_active_statistic()
        {
            var replica = new Uri("http://r1");
            var replicaResult = Accepted(replica.OriginalString, 500);
            
            relativeWeightModifier.Learn(replicaResult, replicaStorageProvider);

            clusterState.CurrentStatistic.Received(1).Report(replicaResult);
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

        [TestCase("http://r1", 0.140)]
        [TestCase("http://r2", 0.519)]
        [TestCase("http://r3", 0.916)]
        public void Modify_should_correct_update_weights(string replica, double expectedWeight)
        {
            var currentTimestamp = DateTime.UtcNow;
            var previousTimestamp = currentTimestamp - settings.WeightUpdatePeriod;
            var weights = new Dictionary<Uri, Weight>();
            clusterState.Weights
                .When(w => w.Update(Arg.Any<IReadOnlyDictionary<Uri, Weight>>()))
                .Do(info =>
                {
                    foreach (var weight in info.Arg<IReadOnlyDictionary<Uri, Weight>>())
                        weights[weight.Key] = weight.Value;
                });
            clusterState.Weights.Get(Arg.Any<Uri>()).Returns(
                c =>
                {
                    var ww = weights.TryGetValue(c.Arg<Uri>(), out var w) ? w : (Weight?)null;
                    return ww;
                });
            statisticHistory.GetForCluster().Returns(new Statistic(30, 120, previousTimestamp));
            statisticHistory.GetForReplica(new Uri("http://r1")).Returns(new Statistic(90, 300, previousTimestamp));
            statisticHistory.GetForReplica(new Uri("http://r2")).Returns(new Statistic(40, 200, previousTimestamp));
            statisticHistory.GetForReplica(new Uri("http://r3")).Returns(new Statistic(50, 180, previousTimestamp));
            clusterState.CurrentStatistic
                .ObserveCluster(Arg.Any<DateTime>(), Arg.Any<double>(), Arg.Any<Statistic?>())
                .Returns(info => new Statistic(70, 300, info.Arg<DateTime>()));
            clusterState.CurrentStatistic
                .ObserveReplicas(Arg.Any<DateTime>(), Arg.Any<double>(), Arg.Any<Func<Uri, Statistic?>>())
                .Returns( c =>
                    new List<(Uri Replica, Statistic Statistic)>()
                    {
                        (new Uri("http://r1"), new Statistic(125, 500, c.Arg<DateTime>())),
                        (new Uri("http://r2"), new Statistic(70, 250, c.Arg<DateTime>())),
                        (new Uri("http://r3"), new Statistic(60, 190, c.Arg<DateTime>()))
                    });

            var actualWeight = 1.0d;
            Thread.Sleep(settings.WeightUpdatePeriod);
            relativeWeightModifier.Modify(new Uri(replica), new List<Uri>(), replicaStorageProvider, Request.Get(""), RequestParameters.Empty, ref actualWeight);

            actualWeight.Should().BeApproximately(expectedWeight, 0.001);
        }

        private static ReplicaResult Accepted(string replica,int time) =>
            new ReplicaResult(new Uri(replica), new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
    }
}