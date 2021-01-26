using System.Threading;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Modules
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
        /// Gets or sets used request parameters.
        /// </summary>
        [NotNull]
        RequestParameters Parameters { get; set; }

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
        /// Gets or sets ClusterProvider instance used to send request.
        /// </summary>
        [NotNull]
        IClusterProvider ClusterProvider { get; set; }

        /// <summary>
        /// Gets or sets ReplicaOrdering instance used to send request.
        /// </summary>
        [NotNull]
        IReplicaOrdering ReplicaOrdering { get; set; }

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
        /// Gets or sets the number of connection attempts to each replica.
        /// </summary>
        int ConnectionAttempts { get; set; }

        /// <summary>
        /// Gets the client application name.
        /// </summary>
        string ClientApplicationName { get; }

        /// <summary>
        /// Clear replica results info.
        /// </summary>
        void ResetReplicaResults();
    }
}