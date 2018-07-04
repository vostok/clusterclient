using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects any response.
    /// </summary>
    public class AlwaysRejectCriterion : IResponseCriterion
    {
        public ResponseVerdict Decide(Response response) => ResponseVerdict.Reject;
    }
}