using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which accepts any response with <see cref="HeaderNames.XVostokDontRetry" /> header.
    /// </summary>
    public class AcceptNonRetriableCriterion : IResponseCriterion
    {
        public ResponseVerdict Decide(Response response) =>
            response.Headers[HeaderNames.XVostokDontRetry] != null ? ResponseVerdict.Accept : ResponseVerdict.DontKnow;
    }
}