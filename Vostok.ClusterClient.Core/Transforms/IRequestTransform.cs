using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Transforms
{
    /// <summary>
    /// <para>Represents a transform used to modify request before it gets sent.</para>
    /// <para>Requests transforms form a chain where each transform works with a result of previous one.</para>
    /// </summary>
    [PublicAPI]
    public interface IRequestTransform : IRequestTransformMetadata
    {
        /// <summary>
        /// Implementations of this method MUST BE thread-safe.
        /// </summary>
        [Pure]
        [NotNull]
        Request Transform([NotNull] Request request);
    }

    /// <summary>
    /// <para>Represents a transform used to modify request before it gets sent.</para>
    /// <para>Requests transforms form a chain where each transform works with a result of previous one.</para>
    /// </summary>
    [PublicAPI]
    public interface IAsyncRequestTransform : IRequestTransformMetadata
    {
        /// <summary>
        /// Implementations of this method MUST BE thread-safe.
        /// </summary>
        [Pure]
        [NotNull]
        Task<Request> TransformAsync([NotNull] Request request);
    }

    /// <summary>
    /// Marker interface for request transformations applied in the <see cref="Vostok.Clusterclient.Core.Modules.RequestTransformationModule"/>
    /// </summary>
    public interface IRequestTransformMetadata
    {
    }
}