using System;
using System.Collections.Generic;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    // CR(m_kiskachi) По сути, существует сырая статистика — информация о количестве запросов и латенси.
    // И агрегированная статистика — средние и отклонения посчитанные на основании сырой.
    // Но в названиях классов вместо этого используется слово Active, которое
    // добавляет привязку ко времени и сбивает с толку.

    // AggregatedClusterStatistic
    internal class ClusterStatistic
    {
        // CR(m_kiskachi) Statistic  -> AggregatedStatistic
        public readonly Statistic Cluster;
        public readonly IReadOnlyDictionary<Uri, Statistic> Replicas;

        public ClusterStatistic(
            Statistic cluster, 
            IReadOnlyDictionary<Uri, Statistic> replicas)
        {
            Cluster = cluster;
            Replicas = replicas;
        }
    }
}