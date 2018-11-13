using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Leadership
{
    /// <summary>
    /// Determines leader of cluster by <see cref="ReplicaResult"/>.
    /// </summary>
    [PublicAPI]
    public interface ILeaderResultDetector
    {
        /// <summary>
        /// <para>Returns <c>true</c> if given <paramref name="result"/> definitely belongs to a leader of cluster, or <c>false otherwise</c>.</para>
        /// <para>See <see cref="LeadershipWeightModifier"/> for more details about assumed leader-reservist model.</para>
        /// </summary>
        [Pure]
        bool IsLeaderResult([NotNull] ReplicaResult result);
    }
}