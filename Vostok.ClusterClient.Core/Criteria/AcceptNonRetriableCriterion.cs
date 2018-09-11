using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which accepts any response with <see cref="HeaderNames.DontRetry" /> header.
    /// </summary>
    public class AcceptNonRetriableCriterion : IResponseCriterion
    {
        public ResponseVerdict Decide(Response response) =>
            response.Headers[HeaderNames.DontRetry] != null ? ResponseVerdict.Accept : ResponseVerdict.DontKnow;
    }
}