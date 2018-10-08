using System.Collections.Generic;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    internal class ResponseClassifier : IResponseClassifier
    {
        public ResponseVerdict Decide(Response response, IList<IResponseCriterion> criteria)
        {
            foreach (var c in criteria)
            {
                var verdict = c.Decide(response);
                if (verdict != ResponseVerdict.DontKnow)
                    return verdict;
            }

            return ResponseVerdict.DontKnow;
        }
    }
}