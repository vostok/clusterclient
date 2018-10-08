using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    internal interface IResponseClassifier
    {
        [Pure]
        ResponseVerdict Decide([NotNull] Response response, [NotNull] IList<IResponseCriterion> criteria);
    }
}