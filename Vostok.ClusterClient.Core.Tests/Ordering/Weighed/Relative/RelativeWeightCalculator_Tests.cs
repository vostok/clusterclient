using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class RelativeWeightCalculator_Tests
    {
        private RelativeWeightSettings settings;
        private RelativeWeightCalculator relativeWeightCalculator;

        [SetUp]
        public void Setup()
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
            relativeWeightCalculator = new RelativeWeightCalculator(settings);
        }

        [TestCase(125, 500, 0.140)]
        [TestCase(70, 250, 0.423)]
        [TestCase(60, 190, 0.731)]
        [TestCase(0, 0, 1.0)]
        public void Should_correct_calculate_weights_by_latency(double replicaStdDev, double replicaAvg, double expectedWeight)
        {
            var timeStamp = DateTime.UtcNow;
            var clusterStatistic = new AggregatedStatistic(1000, 0.2, 70, 300, timeStamp);
            var replicaStatistic = new AggregatedStatistic(1000, 0.2, replicaStdDev, replicaAvg, timeStamp);

            var calculatedWeight = relativeWeightCalculator
                .Calculate(clusterStatistic, replicaStatistic, new Weight(settings.InitialWeight, timeStamp - settings.WeightUpdatePeriod));

            calculatedWeight.Value.Should().BeApproximately(expectedWeight, 0.001);
            calculatedWeight.Timestamp.Should().Be(timeStamp);
        }

        [TestCase(33, 0.35, 0.005)]
        [TestCase(33, 0.15, 0.402)]
        [TestCase(33, 0.05, 0.936)]
        public void Should_correct_calculate_weights_by_statuses(double replicaTotalCount, double replicaErrorFraction, double expectedWeight)
        {
            settings.WeightUpdatePeriod = 10.Seconds();
            settings.WeightByStatusesRpsThreshold = 10;
            var timeStamp = DateTime.UtcNow;
            var clusterStatistic = new AggregatedStatistic(100, 0.2, 70, 300, timeStamp);
            var replicaStatistic = new AggregatedStatistic(replicaTotalCount, replicaErrorFraction, 10000, 1000, timeStamp);

            var calculatedWeight = relativeWeightCalculator
                .Calculate(clusterStatistic, replicaStatistic, new Weight(settings.InitialWeight, timeStamp - settings.WeightUpdatePeriod));

            calculatedWeight.Value.Should().BeApproximately(expectedWeight, 0.001);
            calculatedWeight.Timestamp.Should().Be(timeStamp);
        }
    }
}