using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Leadership
{
    /// <summary>
    /// Represents a leader result detector which accepts any result with <see cref="ResponseVerdict.Accept"/> verdict.
    /// </summary>
    [PublicAPI]
    public class AcceptedVerdictLeaderDetector : ILeaderResultDetector
    {
        /// <inheritdoc />
        public bool IsLeaderResult(ReplicaResult result) =>
            result.Verdict == ResponseVerdict.Accept;
    }
}