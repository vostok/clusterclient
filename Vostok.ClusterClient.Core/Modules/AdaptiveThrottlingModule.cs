using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Helpers;
using Vostok.Commons.Threading;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// An implementation of adaptive client throttling mechanism described in https://landing.google.com/sre/book/chapters/handling-overload.html.
    /// </summary>
    internal class AdaptiveThrottlingModule : IRequestModule
    {
        private static readonly ConcurrentDictionary<string, Counter> Counters = new ConcurrentDictionary<string, Counter>();
        private static readonly Stopwatch Watch = Stopwatch.StartNew();

        public static void ClearCache()
        {
            Counters.Clear();
        }

        private readonly AdaptiveThrottlingOptions options;
        private readonly Func<string, Counter> counterFactory;

        public AdaptiveThrottlingModule(AdaptiveThrottlingOptions options)
        {
            this.options = options;
            counterFactory = _ => new Counter(options.MinutesToTrack);
        }

        public int Requests => GetCounter().GetMetrics().Requests;

        public int Accepts => GetCounter().GetMetrics().Accepts;

        public double Ratio => ComputeRatio(GetCounter().GetMetrics());

        public double RejectionProbability => ComputeRejectionProbability(GetCounter().GetMetrics(), options);

        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var counter = GetCounter();

            counter.BeginRequest();

            ClusterResult result;

            try
            {
                counter.AddRequest();

                double ratio;
                double rejectionProbability;

                var metrics = counter.GetMetrics();
                if (metrics.Requests >= options.MinimumRequests &&
                    (ratio = ComputeRatio(metrics)) >= options.CriticalRatio &&
                    (rejectionProbability = ComputeRejectionProbability(metrics, options)) > ThreadSafeRandom.NextDouble())
                {
                    LogThrottledRequest(context, ratio, rejectionProbability);

                    return ClusterResult.Throttled(context.Request);
                }

                result = await next(context).ConfigureAwait(false);

                UpdateCounter(counter, result);
            }
            catch (OperationCanceledByServerException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                counter.AddAccept();
                throw;
            }
            finally
            {
                counter.EndRequest();
            }
            return result;
        }

        private Counter GetCounter() => Counters.GetOrAdd(options.StorageKey, counterFactory);

        private static double ComputeRatio(CounterMetrics metrics) =>
            1.0 * metrics.Requests / Math.Max(1.0, metrics.Accepts);

        private static double ComputeRejectionProbability(CounterMetrics metrics, AdaptiveThrottlingOptions options)
        {
            var probability = 1.0 * (metrics.Requests - options.CriticalRatio * metrics.Accepts) / (metrics.Requests + 1);

            probability = Math.Max(probability, 0.0);
            probability = Math.Min(probability, options.MaximumRejectProbability);

            return probability;
        }

        private static void UpdateCounter(Counter counter, ClusterResult result)
        {
            if (result.ReplicaResults.Any(r => r.Verdict == ResponseVerdict.Accept))
                counter.AddAccept();
        }

        #region Logging

        private void LogThrottledRequest(IRequestContext context, double ratio, double rejectionProbability) =>
            context.Log.Warn($"Throttled request without sending it. Request/accept ratio = {ratio:F3}. Rejection probability = {rejectionProbability:F3}");

        #endregion

        #region CounterMetrics

        private struct CounterMetrics
        {
            public int Requests;
            public int Accepts;
        }

        #endregion

        #region CounterBucket

        private class CounterBucket
        {
            public volatile int Minute;
            public int Requests;
            public int Accepts;
        }

        #endregion

        #region Counter

        private class Counter
        {
            private readonly CounterBucket[] buckets;
            private int pendingRequests;

            public Counter(int buckets)
            {
                this.buckets = new CounterBucket[buckets];

                for (var i = 0; i < buckets; i++)
                    this.buckets[i] = new CounterBucket();
            }

            public CounterMetrics GetMetrics()
            {
                var metrics = new CounterMetrics();
                var minute = GetCurrentMinute();

                foreach (var bucket in buckets)
                {
                    if (bucket.Minute <= minute - buckets.Length)
                        continue;

                    metrics.Requests += bucket.Requests;
                    metrics.Accepts += bucket.Accepts;
                }

                metrics.Requests -= pendingRequests;

                return metrics;
            }

            public void BeginRequest() => Interlocked.Increment(ref pendingRequests);

            public void EndRequest() => Interlocked.Decrement(ref pendingRequests);

            public void AddRequest() => Interlocked.Increment(ref ObtainBucket().Requests);

            public void AddAccept() => Interlocked.Increment(ref ObtainBucket().Accepts);

            private CounterBucket ObtainBucket()
            {
                var minute = GetCurrentMinute();
                var bucketIndex = minute % buckets.Length;

                while (true)
                {
                    var currentBucket = buckets[bucketIndex];
                    if (currentBucket.Minute >= minute)
                        return currentBucket;

                    Interlocked.CompareExchange(ref buckets[bucketIndex], new CounterBucket {Minute = minute}, currentBucket);
                }
            }

            private static int GetCurrentMinute() => (int)Math.Floor(Watch.Elapsed.TotalMinutes);
        }

        #endregion
    }
}