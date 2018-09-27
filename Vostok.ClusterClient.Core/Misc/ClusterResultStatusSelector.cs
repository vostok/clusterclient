using System.Collections.Generic;
using System.Linq;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Misc
{
    internal class ClusterResultStatusSelector : IClusterResultStatusSelector
    {
        public ClusterResultStatus Select(IList<ReplicaResult> results, IRequestTimeBudget budget)
        {
            if (results.Any(result => result.Verdict == ResponseVerdict.Accept))
                return ClusterResultStatus.Success;

            if (budget.HasExpired)
                return ClusterResultStatus.TimeExpired;

            return ClusterResultStatus.ReplicasExhausted;
        }
    }
}