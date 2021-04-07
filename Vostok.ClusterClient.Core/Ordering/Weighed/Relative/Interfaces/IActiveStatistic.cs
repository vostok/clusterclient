using System;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IActiveStatistic
    {
        double CalculatePenalty();

        void Report(ReplicaResult replicaResult);

        ClusterStatistic CalculateClusterStatistic(DateTime currentTime, double penalty, ClusterStatistic previous);
    }
}