using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Ordering.Weighed
{
    /// <summary>
    /// Represents a builder used to construct a <see cref="WeighedReplicaOrdering"/> instance.
    /// </summary>
    [PublicAPI]
    public interface IWeighedReplicaOrderingBuilder
    {
        /// <summary>
        /// Gets log instance.
        /// </summary>
        ILog Log { get; }

        /// <summary>
        /// Gets or sets the minimum possible replica weight. It must be greater or equal to zero.
        /// </summary>
        double MinimumWeight { get; set; }

        /// <summary>
        /// Gets or sets the maximum possible replica weight. It must be greater or equal to <see cref="MinimumWeight"/>.
        /// </summary>
        double MaximumWeight { get; set; }

        /// <summary>
        /// Gets or sets the initial (default) replica weight. It must be between <see cref="MinimumWeight"/> and <see cref="MaximumWeight"/>.
        /// </summary>
        double InitialWeight { get; set; }

        /// <summary>
        /// <para>Gets or sets the name of service this <see cref="ClusterClient"/> will talk to.</para>
        /// <para>This parameter is taken from <see cref="IClusterClientConfiguration"/>.</para>
        /// </summary>
        string ServiceName { get; set; }

        /// <summary>
        /// <para>Gets or sets the environment of service this <see cref="ClusterClient"/> will talk to.</para>
        /// <para>This parameter is taken from <see cref="IClusterClientConfiguration"/>.</para>
        /// </summary>
        string Environment { get; set; }

        /// <summary>
        /// Adds given <paramref name="modifier"/> to the modifiers chain.
        /// </summary>
        void AddModifier([NotNull] IReplicaWeightModifier modifier);
    }
}