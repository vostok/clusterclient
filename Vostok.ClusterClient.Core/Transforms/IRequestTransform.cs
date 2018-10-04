using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// <para>Represents a transform used to modify request before it gets sent.</para>
    /// <para>Requests transforms form a chain where each transform works with a result of previous one.</para>
    /// </summary>
    [PublicAPI]
    public interface IRequestTransform
    {
        /// <summary>
        /// Implementations of this method MUST BE thread-safe.
        /// </summary>
        [Pure]
        [NotNull]
        Request Transform([NotNull] Request request);
    }
}