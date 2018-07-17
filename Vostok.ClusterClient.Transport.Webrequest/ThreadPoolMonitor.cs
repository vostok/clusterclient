using System;
using System.Threading;
using Vostok.Commons.ThreadManagment;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal class ThreadPoolMonitor
    {
        private const int TargetMultiplier = 128;

        public static readonly ThreadPoolMonitor Instance = new ThreadPoolMonitor();

        private static readonly TimeSpan MinReportInterval = TimeSpan.FromSeconds(1);

        private readonly object syncObject;
        private DateTime lastReportTimestamp;

        public ThreadPoolMonitor()
        {
            syncObject = new object();
            lastReportTimestamp = DateTime.MinValue;
        }

        public void ReportAndFixIfNeeded(ILog log)
        {
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minIocpThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxIocpThreads);
            ThreadPool.GetAvailableThreads(out var availableWorkerThreads, out var availableIocpThreads);

            var busyWorkerThreads = maxWorkerThreads - availableWorkerThreads;
            var busyIocpThreads = maxIocpThreads - availableIocpThreads;

            if (busyWorkerThreads < minWorkerThreads && busyIocpThreads < minIocpThreads)
                return;

            var currentTimestamp = DateTime.UtcNow;

            lock (syncObject)
            {
                if (currentTimestamp - lastReportTimestamp < MinReportInterval)
                    return;

                lastReportTimestamp = currentTimestamp;
            }

            // log = log.WithPrefix(GetType().Name);
            // todo(Mansiper): fix it
            log = log.WithContextualPrefix();
            using (new ContextualLogPrefix(GetType().Name))
                log.Warn(
                    "Looks like you're kinda low on ThreadPool, buddy. Workers: {0}/{1}/{2}, IOCP: {3}/{4}/{5} (busy/min/max).",
                    busyWorkerThreads,
                    minWorkerThreads,
                    maxWorkerThreads,
                    busyIocpThreads,
                    minIocpThreads,
                    maxIocpThreads);

            var currentMultiplier = Math.Min(minWorkerThreads/Environment.ProcessorCount, minIocpThreads/Environment.ProcessorCount);
            if (currentMultiplier < TargetMultiplier)
            {
                using (new ContextualLogPrefix(GetType().Name))
                    log.Info("I will configure ThreadPool for you, buddy!");
                ThreadPoolUtility.SetUp(TargetMultiplier);
            }
        }
    }
}