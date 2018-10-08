using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects responses with <see cref="ResponseCode.Unknown"/>, and <see cref="ResponseCode.UnknownFailure"/> codes.
    /// </summary>
    [PublicAPI]
    public class RejectUnknownErrorsCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response)
        {
            switch (response.Code)
            {
                case ResponseCode.Unknown:
                case ResponseCode.UnknownFailure:
                    return ResponseVerdict.Reject;

                default:
                    return ResponseVerdict.DontKnow;
            }
        }
    }
}