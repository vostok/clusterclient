using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    internal interface IResponseClassifier
    {
        [Pure]
        ResponseVerdict Decide([NotNull] Response response, [NotNull] IList<IResponseCriterion> criteria);
    }
}