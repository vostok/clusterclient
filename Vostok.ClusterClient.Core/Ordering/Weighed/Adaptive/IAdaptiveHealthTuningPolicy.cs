using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive
{
    /// <summary>
    /// Represents a policy used to take different action on replica health values based on request results.
    /// </summary>
    [PublicAPI]
    public interface IAdaptiveHealthTuningPolicy
    {
        /// <summary>
        /// <para>Selects and returns an appropriate action to be taken on health of replica from given <paramref name="result"/>. See <see cref="AdaptiveHealthAction"/> for details.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        [Pure]
        AdaptiveHealthAction SelectAction([NotNull] ReplicaResult result);
    }
}