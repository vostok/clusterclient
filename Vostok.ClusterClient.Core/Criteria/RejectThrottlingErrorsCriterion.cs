using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects responses with <see cref="ResponseCode.TooManyRequests"/> code.
    /// </summary>
    [PublicAPI]
    public class RejectThrottlingErrorsCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response) =>
            response.Code == ResponseCode.TooManyRequests ? ResponseVerdict.Reject : ResponseVerdict.DontKnow;
    }
}