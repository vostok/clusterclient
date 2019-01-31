using System;
using Vostok.Commons.Threading;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Misc
{
    internal static class ThreadPoolMonitor
    {
        private const int TargetMultiplier = 128;

        private static readonly TimeSpan MinReportInterval = TimeSpan.FromSeconds(1);

        private static readonly object syncObject = new object();

        private static DateTime lastReportTimestamp = DateTime.MinValue;

        public static void ReportAndFixIfNeeded(ILog log)
        {
            var state = ThreadPoolUtility.GetPoolState();

            if (state.UsedWorkerThreads < state.MinWorkerThreads &&
                state.UsedIocpThreads < state.MinIocpThreads)
                return;

            var currentTimestamp = DateTime.UtcNow;

            lock (syncObject)
            {
                if (currentTimestamp - lastReportTimestamp < MinReportInterval)
                    return;

                lastReportTimestamp = currentTimestamp;
            }

            log = log.ForContext(typeof(ThreadPoolMonitor));

            log.Warn(
                "Looks like you're kinda low on ThreadPool, buddy. " +
                "Workers: {UsedWorkerThreads}/{MinWorkerThreads}, " +
                "IOCP: {UsedIocpThreads}/{MinIocpThreads} (busy/min).",
                state.UsedWorkerThreads,
                state.MinWorkerThreads,
                state.UsedIocpThreads,
                state.MinIocpThreads);

            var currentMultiplier = Math.Min(
                state.MinWorkerThreads / Environment.ProcessorCount,
                state.MinIocpThreads / Environment.ProcessorCount);

            if (currentMultiplier < TargetMultiplier)
            {
                log.Info("I will configure ThreadPool for you, buddy!");

                ThreadPoolUtility.Setup(TargetMultiplier);

                var newState = ThreadPoolUtility.GetPoolState();

                log.Info("New min worker threads = {MinWorkerThreads}", newState.MinWorkerThreads);
                log.Info("New min IOCP threads = {MinIocpThreads}", newState.MinIocpThreads);
            }
        }
    }
}
