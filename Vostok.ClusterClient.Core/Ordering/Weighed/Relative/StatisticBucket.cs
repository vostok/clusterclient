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
            var rejected = Interlocked.Read(ref rejectCount);
            var rejectSum = InterlockedEx.Read(ref rejectLatencySum);
            var rejectSquareSum = InterlockedEx.Read(ref rejectLatencySquaredSum);
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
                Interlocked.Increment(ref rejectCount);
                InterlockedEx.Add(ref rejectLatencySum, result.Time.TotalMilliseconds);
                InterlockedEx.Add(ref rejectLatencySquaredSum, result.Time.TotalMilliseconds * result.Time.TotalMilliseconds);
            }
        }

        public AggregatedStatistic Aggregate(DateTime timestamp)
        {
            var mean = (InterlockedEx.Read(ref successLatencySum) + InterlockedEx.Read(ref rejectLatencySum)) / Math.Max(1, Interlocked.Read(ref totalCount));

            var squaredMean = (InterlockedEx.Read(ref successLatencySquaredSum) + InterlockedEx.Read(ref rejectLatencySquaredSum)) / Math.Max(1, Interlocked.Read(ref totalCount));
            var variance = Math.Max(0d, squaredMean - mean * mean);

            var stdDev = Math.Sqrt(variance);

            return new AggregatedStatistic(stdDev, mean, timestamp);
        }

        private static double PenalizeLatency(double latency, long count, double penalty) => 
            latency + count * penalty;

        private static double PenalizeSquaredLatency(double latency, double squaredLatency, double count, double penalty) =>
            squaredLatency + 2 * penalty * latency + penalty * penalty * count;
    }
}