using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which accepts any response with <see cref="HeaderNames.DontRetry" /> header.
    /// </summary>
    [PublicAPI]
    public class AcceptNonRetriableCriterion : IResponseCriterion
    {
        /// <inheritdoc />
        public ResponseVerdict Decide(Response response) =>
            response.Headers[HeaderNames.DontRetry] != null ? ResponseVerdict.Accept : ResponseVerdict.DontKnow;
    }
}