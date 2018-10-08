using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which accepts any response.
    /// </summary>
    [PublicAPI]
    public class AlwaysAcceptCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response) => ResponseVerdict.Accept;
    }
}