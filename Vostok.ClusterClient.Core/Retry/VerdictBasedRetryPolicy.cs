using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Retry
{
    [PublicAPI]
    public class VerdictBasedRetryPolicy : IRetryPolicy
    {
        public bool NeedToRetry(Request request, RequestParameters parameters, IList<ReplicaResult> results)
        {
            return results.Count == 0 ||
                   results.All(result => result.Verdict != ResponseVerdict.Accept && result.Response.Headers[HeaderNames.DontFork] == null);
        }
    }
}