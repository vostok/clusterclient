using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Commons.Collections;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Gray
{
    /// <summary>
    /// <para>Represents a modifier which keeps a list of bad ("gray") replicas. Replicas which are not in this list are called "white".</para>
    /// <para>The weight of white replicas is not modified at all.</para>
    /// <para>The weight of gray replicas is dropped to zero.</para>
    /// <para>A replica is added to gray list when it returns a response with <see cref="ResponseVerdict.Reject"/> verdict.</para>
    /// <para>A replica remains in gray list for a period of time known as gray period, as given by the <see cref="IGrayPeriodProvider"/> implementation.</para>
    /// </summary>
    [PublicAPI]
    public class GrayListModifier : IReplicaWeightModifier
    {
        private static readonly string StorageKey = typeof(GrayListModifier).FullName;

        private readonly IGrayPeriodProvider grayPeriodProvider;
        private readonly Func<DateTime> getCurrentTime;
        private readonly ILog log;

        /// <param name="grayPeriodProvider">A gray periods provider</param>
        /// <param name="log"><see cref="ILog"/> instance.</param>
        public GrayListModifier([NotNull] IGrayPeriodProvider grayPeriodProvider, [CanBeNull] ILog log)
            : this(grayPeriodProvider, () => DateTime.UtcNow, log)
        {
        }

        /// <param name="grayPeriod">A constant gray period.</param>
        /// <param name="log"><see cref="ILog"/> instance.</param>
        public GrayListModifier(TimeSpan grayPeriod, [CanBeNull] ILog log)
            : this(new FixedGrayPeriodProvider(grayPeriod), log)
        {
        }

        internal GrayListModifier([NotNull] IGrayPeriodProvider grayPeriodProvider, [NotNull] Func<DateTime> getCurrentTime, [CanBeNull] ILog log)
        {
            this.grayPeriodProvider = grayPeriodProvider ?? throw new ArgumentNullException(nameof(grayPeriodProvider));
            this.getCurrentTime = getCurrentTime ?? throw new ArgumentNullException(nameof(getCurrentTime));
            this.log = log ?? new SilentLog();
        }

        /// <inheritdoc />
        public void Modify(Uri replica, IList<Uri> allReplicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, ref double weight)
        {
            var storage = storageProvider.Obtain<DateTime>(StorageKey);
            if (!storage.TryGetValue(replica, out var lastGrayTimestamp))
                return;

            var currentTime = getCurrentTime();
            var grayPeriod = grayPeriodProvider.GetGrayPeriod();

            if (lastGrayTimestamp + grayPeriod >= currentTime)
                weight = 0.0;
            else if (storage.Remove(replica, lastGrayTimestamp))
                LogReplicaIsNoLongerGray(replica);
        }

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
            if (result.Verdict != ResponseVerdict.Reject)
                return;

            if (result.Response.Code == ResponseCode.StreamReuseFailure ||
                result.Response.Code == ResponseCode.StreamInputFailure)
                return;

            var storage = storageProvider.Obtain<DateTime>(StorageKey);
            var wasGray = storage.ContainsKey(result.Replica);

            storage[result.Replica] = getCurrentTime();

            if (!wasGray)
                LogReplicaIsGrayNow(result.Replica);
        }

        private void LogReplicaIsGrayNow(Uri replica) =>
            log.Warn($"Replica '{replica}' is now gray.");

        private void LogReplicaIsNoLongerGray(Uri replica) =>
            log.Info($"Replica '{replica}' is no longer gray.");
    }
}