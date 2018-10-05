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
        LeakPrevention = 1,
        /// <summary>
        /// A module which handle and log errors.
        /// </summary>
        GlobalErrorCatching = 2,
        /// <summary>
        /// A module which performs request transformation (applies <see cref="IRequestTransform"/> chain).
        /// </summary>
        RequestTransformation = 3,
        /// <summary>
        /// A module which applies request priority (add a <see cref="HeaderNames.RequestPriority"/> header to request). 
        /// </summary>
        RequestPriority = 4,
        /// <summary>
        /// A module which applies client application identity (add a <see cref="HeaderNames.ClientApplication"/> header to request).
        /// </summary>
        ClientApplication = 5,
        /// <summary>
        /// A module which log requests and responses.
        /// </summary>
        Logging = 6,
        /// <summary>
        /// A module which performs response transformation (applies <see cref="IResponseTransform"/> chain).
        /// </summary>
        ResponseTransformation = 7,
        /// <summary>
        /// A module which handle and log errors.
        /// </summary>
        ErrorCatching = 8,
        /// <summary>
        /// A module which validates request.
        /// </summary>
        RequestValidation = 9,
        /// <summary>
        /// A module which validates request timeout.
        /// </summary>
        TimeoutValidation = 10,
        /// <summary>
        /// A module which send request in try loop with <see cref="IRetryPolicy"/> and <see cref="IRetryStrategy"/>.
        /// </summary>
        RequestRetry = 11,
        /// <summary>
        /// A module which send requests with absolute urls (directly using <see cref="ITransport"/>).
        /// </summary>
        AbsoluteUrlSender = 12,
        /// <summary>
        /// A module which execute requests (<see cref="IClusterProvider"/> --> <see cref="IReplicaOrdering"/> --> <see cref="IRequestStrategy"/>)
        /// </summary>
        RequestExecution = 13,
        /// <inheritdoc cref="AdaptiveThrottlingModule" />
        AdaptiveThrottling = 14,
        /// <inheritdoc cref="ReplicaBudgetingModule" />
        ReplicaBudgeting = 15
    }
}