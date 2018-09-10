using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Ordering.Storage;
using Vostok.ClusterClient.Core.Helpers;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering.Storage;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Gray
{
    /// <summary>
    /// <para>Represents a modifier which keeps a list of bad ("gray") replicas. Replicas which are not in this list are called "white".</para>
    /// <para>The weight of white replicas is not modified at all.</para>
    /// <para>The weight of gray replicas is dropped to zero.</para>
    /// <para>A replica is added to gray list when it returns a response with <see cref="ResponseVerdict.Reject"/> verdict.</para>
    /// <para>A replica remains in gray list for a period of time known as gray period, as given by the <see cref="IGrayPeriodProvider"/> implementation.</para>
    /// </summary>
    public class GrayListModifier : IReplicaWeightModifier
    {
        private static readonly string StorageKey = typeof(GrayListModifier).FullName;

        private readonly IGrayPeriodProvider grayPeriodProvider;
        private readonly ITimeProvider timeProvider;
        private readonly ILog log;

        public GrayListModifier([NotNull] IGrayPeriodProvider grayPeriodProvider, [CanBeNull] ILog log)
            : this(grayPeriodProvider, new TimeProvider(), log)
        {
        }

        public GrayListModifier(TimeSpan grayPeriod, [CanBeNull] ILog log)
            : this(new FixedGrayPeriodProvider(grayPeriod), log)
        {
        }

        internal GrayListModifier([NotNull] IGrayPeriodProvider grayPeriodProvider, [NotNull] ITimeProvider timeProvider, [CanBeNull] ILog log)
        {
            this.grayPeriodProvider = grayPeriodProvider ?? throw new ArgumentNullException(nameof(grayPeriodProvider));
            this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            this.log = log ?? new SilentLog();
        }

        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, ref double weight)
        {
            var storage = storageProvider.Obtain<DateTime>(StorageKey);
            if (!storage.TryGetValue(replica, out var lastGrayTimestamp))
                return;

            var currentTime = timeProvider.GetCurrentTime();
            var grayPeriod = grayPeriodProvider.GetGrayPeriod();

            if (lastGrayTimestamp + grayPeriod >= currentTime)
                weight = 0.0;
            else if (storage.Remove(replica, lastGrayTimestamp))
                LogReplicaIsNoLongerGray(replica);
        }

        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
            if (result.Verdict != ResponseVerdict.Reject)
                return;

            if (result.Response.Code == ResponseCode.StreamReuseFailure ||
                result.Response.Code == ResponseCode.StreamInputFailure)
                return;

            var storage = storageProvider.Obtain<DateTime>(StorageKey);
            var wasGray = storage.ContainsKey(result.Replica);

            storage[result.Replica] = timeProvider.GetCurrentTime();

            if (!wasGray)
                LogReplicaIsGrayNow(result.Replica);
        }

        private void LogReplicaIsGrayNow(Uri replica) =>
            log.Warn($"Replica '{replica}' is now gray.");

        private void LogReplicaIsNoLongerGray(Uri replica) =>
            log.Info($"Replica '{replica}' is no longer gray.");
    }
}