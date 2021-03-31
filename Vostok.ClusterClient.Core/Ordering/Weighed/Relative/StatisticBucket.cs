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
        private double rejectLatencySum;
        private double rejectLatencySquaredSum;

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
            this.rejectLatencySum = rejectLatencySum;
            this.rejectLatencySquaredSum = rejectLatencySquaredSum;
        }

        public StatisticBucket Penalize(double penalty)
        {
            var total = Interlocked.Read(ref totalCount);
            var successSum = InterlockedEx.Read(ref successLatencySum);
            var successSquareSum = InterlockedEx.Read(ref successLatencySquaredSum);
            var rejectSum = InterlockedEx.Read(ref rejectLatencySum);
            var rejectSquareSum = InterlockedEx.Read(ref rejectLatencySquaredSum);
            var rejected = Interlocked.Read(ref rejectCount);
            return new StatisticBucket(
                total,
                rejected,
                successSum,
                successSquareSum,
                PenalizeLatency(
                    rejectSum,
                    rejected,
                    penalty),
                PenalizeSquaredLatency(
                    rejectSum,
                    rejectSquareSum,
                    rejected,
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
                InterlockedEx.Add(ref rejectLatencySum, result.Time.TotalMilliseconds);
                InterlockedEx.Add(ref rejectLatencySquaredSum, result.Time.TotalMilliseconds * result.Time.TotalMilliseconds);
                Interlocked.Increment(ref rejectCount);
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