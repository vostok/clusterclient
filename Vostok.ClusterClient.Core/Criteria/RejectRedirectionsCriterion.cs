using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects redirection (3xx) responses except for <see cref="ResponseCode.NotModified"/> code.
    /// </summary>
    public class RejectRedirectionsCriterion : IResponseCriterion
    {
        public ResponseVerdict Decide(Response response) =>
            response.Code.IsRedirection() && response.Code != ResponseCode.NotModified
                ? ResponseVerdict.Reject
                : ResponseVerdict.DontKnow;
    }
}