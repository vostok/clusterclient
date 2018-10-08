using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects redirection (3xx) responses except for <see cref="ResponseCode.NotModified"/> code.
    /// </summary>
    [PublicAPI]
    public class RejectRedirectionsCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response) =>
            response.Code.IsRedirection() && response.Code != ResponseCode.NotModified
                ? ResponseVerdict.Reject
                : ResponseVerdict.DontKnow;
    }
}