using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Misc
{
    internal interface IClusterResultStatusSelector
    {
        ClusterResultStatus Select([NotNull] IList<ReplicaResult> results, [NotNull] IRequestTimeBudget budget);
    }
}