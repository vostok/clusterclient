using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Criteria
{
    /// <summary>
    /// Represents a criterion which rejects any response with <see cref="HeaderNames.DontAccept" /> header.
    /// </summary>
    [PublicAPI]
    public class RejectNonAcceptableCriterion : RejectHeaderCriterion
    {
        public RejectNonAcceptableCriterion()
            : base(HeaderNames.DontAccept)
        {
        }
    }
}
