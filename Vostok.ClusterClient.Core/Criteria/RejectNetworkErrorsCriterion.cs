using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects responses that indicate network errors (see <see cref="ResponseCodeExtensions.IsNetworkError"/>).
    /// </summary>
    [PublicAPI]
    public class RejectNetworkErrorsCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response) =>
            response.Code.IsNetworkError() ? ResponseVerdict.Reject : ResponseVerdict.DontKnow;
    }
}