using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which accepts any response with <see cref="HeaderNames.DontRetry" /> header.
    /// </summary>
    [PublicAPI]
    public class AcceptNonRetriableCriterion : AcceptHeaderCriterion
    {
        public AcceptNonRetriableCriterion([NotNull] string headerName = HeaderNames.DontRetry)
            : base(headerName)
        {
        }
    }
}