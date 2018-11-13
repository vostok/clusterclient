using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Leadership
{
    /// <summary>
    /// Represents a leader result detector which accepts any result with <see cref="ResponseVerdict.Accept"/> verdict or <see cref="ResponseCode.ServiceUnavailable"/> response code.
    /// </summary>
    [PublicAPI]
    public class AcceptedVerdictOr503LeaderDetector : ILeaderResultDetector
    {
        /// <inheritdoc />
        public bool IsLeaderResult(ReplicaResult result) =>
            result.Verdict == ResponseVerdict.Accept || result.Response.Code == ResponseCode.ServiceUnavailable;
    }
}