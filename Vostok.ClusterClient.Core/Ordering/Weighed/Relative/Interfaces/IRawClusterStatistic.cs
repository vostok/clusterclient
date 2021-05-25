using System;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IRawClusterStatistic
    {
        void Report(ReplicaResult replicaResult);

        AggregatedClusterStatistic GetPenalizedAndSmoothedStatistic(DateTime currentTime, AggregatedClusterStatistic previous);
    }
}