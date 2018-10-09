using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Retry;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transforms;
using Vostok.Clusterclient.Core.Transport;

namespace Vostok.Clusterclient.Core.Modules
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
        /// A module which adds auxiliary headers such as <see cref="HeaderNames.RequestPriority"/> and <see cref="HeaderNames.ApplicationIdentity"/> to the request). 
        /// </summary>
        AuxiliaryHeaders,
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