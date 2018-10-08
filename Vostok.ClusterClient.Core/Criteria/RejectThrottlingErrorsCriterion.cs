using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
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