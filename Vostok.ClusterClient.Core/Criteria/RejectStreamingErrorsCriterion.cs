using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects responses with <see cref="ResponseCode.StreamInputFailure"/> and <see cref="ResponseCode.StreamReuseFailure"/> codes.
    /// </summary>
    public class RejectStreamingErrorsCriterion : IResponseCriterion
    {
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