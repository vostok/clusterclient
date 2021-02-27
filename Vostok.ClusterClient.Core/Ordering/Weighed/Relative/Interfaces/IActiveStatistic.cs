using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative.Interfaces
{
    internal interface IActiveStatistic
    {
        double CalculatePenalty();

        void Report(ReplicaResult replicaResult);

        Statistic ObserveCluster(DateTime currentTime, double penalty, in Statistic? previous);

        IEnumerable<(Uri Replica, Statistic Statistic)> ObserveReplicas(
            DateTime currentTime,
            double penalty,
            Func<Uri, Statistic?> previousStatisticProvider);
    }
}