using System.Threading;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;
using Vostok.ClusterClient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// Represents a context of currently executed request.
    /// </summary>
    [PublicAPI]
    public interface IRequestContext
    {
        /// <summary>
        /// Gets or sets request which is being sent.
        /// </summary>
        [NotNull]
        Request Request { get; set; }

        /// <summary>
        /// Returns request time budget. Use <see cref="IRequestTimeBudget.Remaining"/> method to check remaining time.
        /// </summary>
        [NotNull]
        IRequestTimeBudget Budget { get; }

        /// <summary>
        /// Returns an <see cref="ILog"/> instance intended for use in custom modules.
        /// </summary>
        [NotNull]
        ILog Log { get; }

        /// <summary>
        /// Gets or sets the transport instance used to send requests to replicas.
        /// </summary>
        [NotNull]
        ITransport Transport { get; set; }

        /// <summary>
        /// Returns a cancellation token used for request.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets or sets the maximum count of replicas a request may use.
        /// </summary>
        int MaximumReplicasToUse { get; set; }

        /// <summary>
        /// Gets the client application name.
        /// </summary>
        string ClientApplicationName { get; }

        /// <summary>
        /// Gets or sets used request parameters.
        /// </summary>
        RequestParameters Parameters { get; }
    }
}