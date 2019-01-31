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
    /// <para>Describes built-in request pipeline modules.</para>
    /// </summary>
    [PublicAPI]
    public enum RequestModule
    {
        /// <summary>
        /// A module which closes underlying response streams.
        /// </summary>
        LeakPrevention,

        /// <summary>
        /// A module which handles and logs exceptions.
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
        /// A module which fixes undertuned <see cref="System.Threading.ThreadPool"/> limits upon encountering request timeouts.
        /// </summary>
        ThreadPoolTuning,

        /// <summary>
        /// A module which logs requests and responses.
        /// </summary>
        Logging,

        /// <summary>
        /// A module which performs response transformation (applies <see cref="IResponseTransform"/> chain).
        /// </summary>
        ResponseTransformation,

        /// <summary>
        /// A module which handles and logs exceptions.
        /// </summary>
        ErrorCatching,

        /// <summary>
        /// A module which validates request.
        /// </summary>
        RequestValidation,

        /// <summary>
        /// <para>A module which validates Request HTTP methods.</para>
        /// <para>Valid HTTP methods defined in <see cref="RequestMethods"/>.</para>
        /// </summary>
        HttpMethodValidation,

        /// <summary>
        /// A module which validates request timeout.
        /// </summary>
        TimeoutValidation,

        /// <summary>
        /// A module which sends request in try loop with <see cref="IRetryPolicy"/> and <see cref="IRetryStrategy"/>.
        /// </summary>
        RequestRetry,

        /// <inheritdoc cref="AdaptiveThrottlingModule" />
        AdaptiveThrottling,

        /// <summary>
        /// A module which sends requests with absolute urls (directly using <see cref="ITransport"/>).
        /// </summary>
        AbsoluteUrlSender,

        /// <inheritdoc cref="ReplicaBudgetingModule" />
        ReplicaBudgeting,

        /// <summary>
        /// A module which executes requests (<see cref="IClusterProvider"/> --> <see cref="IReplicaOrdering"/> --> <see cref="IRequestStrategy"/>)
        /// </summary>
        RequestExecution
    }
}