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
        private readonly string headerName;

        public AcceptNonRetriableCriterion(string headerName = HeaderNames.DontRetry)
        {
            this.headerName = headerName;
        }

        /// <inheritdoc />
        public ResponseVerdict Decide(Response response) =>
            response.Headers[headerName] != null ? ResponseVerdict.Accept : ResponseVerdict.DontKnow;
    }
}