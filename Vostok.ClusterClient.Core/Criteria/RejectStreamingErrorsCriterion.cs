using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects responses with <see cref="ResponseCode.StreamInputFailure"/> and <see cref="ResponseCode.StreamReuseFailure"/> codes.
    /// </summary>
    [PublicAPI]
    public class RejectStreamingErrorsCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response)
        {
            switch (response.Code)
            {
                case ResponseCode.StreamReuseFailure:
                case ResponseCode.StreamInputFailure:
                    return ResponseVerdict.Reject;

                default:
                    return ResponseVerdict.DontKnow;
            }
        }
    }
}