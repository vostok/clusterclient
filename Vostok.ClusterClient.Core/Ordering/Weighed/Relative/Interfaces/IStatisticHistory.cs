using System;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IStatisticHistory
    {
        Statistic? GetForCluster();
        
        Statistic? GetForReplica(Uri replica);
        
        void Update(StatisticSnapshot snapshot);
    }
}