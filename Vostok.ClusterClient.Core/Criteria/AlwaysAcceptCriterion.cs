using Vostok.ClusterClient.Abstractions.Criteria;
using Vostok.ClusterClient.Abstractions.Model;

namespace Vostok.ClusterClient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which accepts any response.
    /// </summary>
    public class AlwaysAcceptCriterion : IResponseCriterion
    {
        public ResponseVerdict Decide(Response response) => ResponseVerdict.Accept;
    }
}