using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Ordering;
using Vostok.ClusterClient.Core.Retry;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.ClusterClient.Core.Topology;
using Vostok.ClusterClient.Core.Transforms;
using Vostok.ClusterClient.Core.Transport;

namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// <para>Describes build-in request pipeline modules.</para>
    /// </summary>
    [PublicAPI]
    public enum RequestModule
    {
        /// <summary>
        /// A module which closes underlying response streams.
        /// </summary>
        LeakPrevention,
        /// <summary>
        /// A module which handle and log errors.
        /// </summary>
        GlobalErrorCatching,
        /// <summary>
        /// A module which performs request transformation (applies <see cref="IRequestTransform"/> chain).
        /// </summary>
        RequestTransformation,
        /// <summary>
        /// A module which applies request priority (add a <see cref="HeaderNames.RequestPriority"/> header to request). 
        /// </summary>
        RequestPriority,
        /// <summary>
        /// A module which applies client application name (add a <see cref="HeaderNames.ApplicationIdentity"/> header to request).
        /// </summary>
        ApplicationName,
        /// <summary>
        /// A module which log requests and responses.
        /// </summary>
        Logging,
        /// <summary>
        /// A module which performs response transformation (applies <see cref="IResponseTransform"/> chain).
        /// </summary>
        ResponseTransformation,
        /// <summary>
        /// A module which handle and log errors.
        /// </summary>
        ErrorCatching,
        /// <summary>
        /// A module which validates request.
        /// </summary>
        RequestValidation,
        /// <summary>
        /// A module which validates request timeout.
        /// </summary>
        TimeoutValidation,
        /// <summary>
        /// A module which send request in try loop with <see cref="IRetryPolicy"/> and <see cref="IRetryStrategy"/>.
        /// </summary>
        RequestRetry,
        /// <summary>
        /// A module which send requests with absolute urls (directly using <see cref="ITransport"/>).
        /// </summary>
        AbsoluteUrlSender,
        /// <summary>
        /// A module which execute requests (<see cref="IClusterProvider"/> --> <see cref="IReplicaOrdering"/> --> <see cref="IRequestStrategy"/>)
        /// </summary>
        RequestExecution,
        /// <inheritdoc cref="AdaptiveThrottlingModule" />
        AdaptiveThrottling,
        /// <inheritdoc cref="ReplicaBudgetingModule" />
        ReplicaBudgeting,
        /// <summary>
        /// <para>A module which validates Request HTTP methods.</para>
        /// <para>A set of valid HTTP methods stored in <see cref="RequestMethods.All"/>.
        /// </summary>
        HttpMethodValidation
    }
}