using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal class StatisticsHistory
    {
        private Statistic? clusterStatistic;
        private readonly ConcurrentDictionary<Uri, Statistic> replicasHistoryStatistics 
            = new ConcurrentDictionary<Uri, Statistic>();


        public Statistic? Get(Uri replica)
        {
            return replicasHistoryStatistics.TryGetValue(replica, out var statistic) 
                ? statistic 
                : default(Statistic?);
        }

        public Statistic? GetCluster() =>
            clusterStatistic;

        public void Update(Statistic cluster, IReadOnlyDictionary<Uri, Statistic> newHistory)
        {
            clusterStatistic = cluster;

            foreach (var historyStat in newHistory)
                replicasHistoryStatistics
                    .AddOrUpdate(historyStat.Key, historyStat.Value, (uri, statistic) => historyStat.Value);
        }
    }
}