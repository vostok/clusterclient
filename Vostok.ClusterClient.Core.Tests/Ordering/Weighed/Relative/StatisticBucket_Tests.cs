using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed.Relative;

namespace Vostok.Clusterclient.Core.Tests.Ordering.Weighed.Relative
{
    [TestFixture]
    public class StatisticBucket_Tests
    {
        private StatisticBucket statisticBucket;

        [SetUp]
        public void SetUp()
        {
            statisticBucket = new StatisticBucket();
        }

        [TestCase(new []{12, 15, 7, 6, 9, 8, 17, 32}, 13.250, 7.964)]
        public void Should_correct_calculate_statistic(int[] responseTimes, double expectedMean, double expectedStdDev)
        {
            var timestamp = DateTime.UtcNow;
            foreach (var responseTime in responseTimes)
                statisticBucket.Report(Accepted(responseTime));
            
            var statistic = statisticBucket.Observe(timestamp);

            statistic.Timestamp.Should().Be(timestamp);
            statistic.Mean.Should().BeApproximately(expectedMean, 0.001);
            statistic.StdDev.Should().BeApproximately(expectedStdDev, 0.001);
        }

        [TestCase(new []{12, 15, 7, 6, 9, 8, 17, 32}, 9.577, 4.407)]
        public void Should_smooth_statistic(int[] responseTimes, double expectedMean, double expectedStdDev)
        {
            var timestamp = DateTime.UtcNow;
            var previousStatistic = new Statistic(3, 8.125, timestamp - 5.Seconds());
            foreach (var responseTime in responseTimes)
                statisticBucket.Report(Accepted(responseTime));

            var smoothedStat = statisticBucket.ObserveSmoothed(timestamp, 15.Seconds(), previousStatistic);

            smoothedStat.Timestamp.Should().Be(timestamp);
            smoothedStat.Mean.Should().BeApproximately(expectedMean, 0.001);
            smoothedStat.StdDev.Should().BeApproximately(expectedStdDev, 0.001);
        }

        [Test]
        public void ObserveSmoothed_should_return_previous_statistic_if_current_is_empty()
        {
            var timestamp = DateTime.UtcNow;
            var previousStatistic = new Statistic(3, 8.125, timestamp - 5.Seconds());

            var smoothedStat = statisticBucket.ObserveSmoothed(timestamp, 15.Seconds(), previousStatistic);

            smoothedStat.Timestamp.Should().Be(timestamp);
            smoothedStat.Mean.Should().BeApproximately(8.125, 0.001);
            smoothedStat.StdDev.Should().BeApproximately(3, 0.001);
        }

        [TestCase(new[] { 120, 150, 70, 60, 90, 80, 170, 320 }, new int[0], 132.5, 79.647)]
        [TestCase(new[] { 120, 150, 70, 60, 90, 80, 170, 320 }, new []{ 5, 7, 3, 4}, 423.25, 416.294)]
        public void Should_penalize_statistic(int[] responseTimes, int[] badTimes, double expectedMean, double expectedStdDev)
        {
            var timestamp = DateTime.UtcNow;
            foreach (var responseTime in responseTimes)
                statisticBucket.Report(Accepted(responseTime));
            foreach (var responseTime in badTimes)
                statisticBucket.Report(Rejected(responseTime));

            var penalizedStat = statisticBucket.Penalize(1000)
                .Observe(timestamp);

            penalizedStat.Timestamp.Should().Be(timestamp);
            penalizedStat.Mean.Should().BeApproximately(expectedMean, 0.001);
            penalizedStat.StdDev.Should().BeApproximately(expectedStdDev, 0.001);
        }

        private static ReplicaResult Accepted(int time) =>
            new ReplicaResult(new Uri("http://replica1:13888"), new Response(ResponseCode.Accepted), ResponseVerdict.Accept, time.Milliseconds());
        private static ReplicaResult Rejected(int time) =>
            new ReplicaResult(new Uri("http://replica1:13888"), new Response(ResponseCode.InternalServerError), ResponseVerdict.Reject, time.Milliseconds());
    }
}