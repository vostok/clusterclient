using System;
using System.Threading;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class StatisticBucket
    {
        private long totalCount;
        private long rejectCount;

        private double successLatencySum;
        private double successLatencySquaredSum;
        private double rejectLatencySquaredSum;
        private double rejectLatencySum;

        public StatisticBucket() { }

        private StatisticBucket(
            long totalCount,
            long rejectCount,
            double successLatencySum,
            double successLatencySquaredSum,
            double rejectLatencySum,
            double rejectLatencySquaredSum)
        {
            this.totalCount = totalCount;
            this.rejectCount = rejectCount;
            this.successLatencySum = successLatencySum;
            this.successLatencySquaredSum = successLatencySquaredSum;
            this.rejectLatencySquaredSum = rejectLatencySquaredSum;
            this.rejectLatencySum = rejectLatencySum;
        }

        public StatisticBucket Penalize(double penalty)
        {
            return new StatisticBucket(
                totalCount,
                rejectCount,
                successLatencySum,
                successLatencySquaredSum,
                PenalizeLatency(
                    InterlockedEx.Read(ref rejectLatencySum),
                    Interlocked.Read(ref rejectCount),
                    penalty),
                PenalizeSquaredLatency(
                    InterlockedEx.Read(ref rejectLatencySum),
                    InterlockedEx.Read(ref rejectLatencySquaredSum),
                    Interlocked.Read(ref rejectCount),
                    penalty)
            );
        }

        public void Report(ReplicaResult result)
        {
            Interlocked.Increment(ref totalCount);
            if (result.Verdict == ResponseVerdict.Accept)
            {
                InterlockedEx.Add(ref successLatencySum, result.Time.TotalMilliseconds);
                InterlockedEx.Add(ref successLatencySquaredSum, result.Time.TotalMilliseconds * result.Time.TotalMilliseconds);
            }
            else
            {
                Interlocked.Increment(ref rejectCount);
                InterlockedEx.Add(ref rejectLatencySum, result.Time.TotalMilliseconds);
                InterlockedEx.Add(ref rejectLatencySquaredSum, result.Time.TotalMilliseconds * result.Time.TotalMilliseconds);
            }
        }

        public Statistic Observe(DateTime timestamp)
        {
            var mean = (InterlockedEx.Read(ref successLatencySum) + InterlockedEx.Read(ref rejectLatencySum)) / Math.Max(1, Interlocked.Read(ref totalCount));

            var squaredMean = (InterlockedEx.Read(ref successLatencySquaredSum) + InterlockedEx.Read(ref rejectLatencySquaredSum)) / Math.Max(1, Interlocked.Read(ref totalCount));
            var variance = Math.Max(0d, squaredMean - mean * mean);

            var stdDev = Math.Sqrt(variance);

            return new Statistic(stdDev, mean, timestamp);
        }

        public Statistic ObserveSmoothed(DateTime current, TimeSpan smoothingConstant, Statistic? previousStat)
        {
            var rowStatistic = Observe(current);
            if (!previousStat.HasValue)
                return rowStatistic;

            var prevMean = previousStat.Value.Mean;
            var prevStdDev = previousStat.Value.StdDev;
            var prevTime = previousStat.Value.Timestamp;

            if (rowStatistic.IsZero())
                return new Statistic(prevStdDev, prevMean, current);

            var mean = SmoothingHelper.SmoothValue(rowStatistic.Mean, prevMean, current, prevTime, smoothingConstant);
            var stdDev = SmoothingHelper.SmoothValue(rowStatistic.StdDev, prevStdDev, current, prevTime, smoothingConstant);
            return new Statistic(stdDev, mean, current);
        }

        private static double PenalizeLatency(double latency, long count, double penalty) => 
            latency + count * penalty;
        private static double PenalizeSquaredLatency(double latency, double squaredLatency, double count, double penalty) =>
            squaredLatency + 2 * penalty * latency + penalty * penalty * count;
    }
}