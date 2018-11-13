using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Misc
{
    internal interface IClusterResultStatusSelector
    {
        ClusterResultStatus Select([NotNull] IList<ReplicaResult> results, [NotNull] IRequestTimeBudget budget);
    }
}