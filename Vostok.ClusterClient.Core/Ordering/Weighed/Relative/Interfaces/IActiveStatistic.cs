using System;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IActiveStatistic
    {
        void Report(ReplicaResult replicaResult);

        ClusterStatistic GetPenalizedAndSmoothedStatistic(DateTime currentTime, ClusterStatistic previous);
    }
}