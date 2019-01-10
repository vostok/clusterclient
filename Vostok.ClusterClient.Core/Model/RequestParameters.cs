using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// <para><see cref="RequestParameters"/> is the primary extension point used to pass per-request info to <see cref="IClusterClient"/>'s <see cref="IClusterClient.SendAsync"/> method.</para>
    /// <para>It contains some of the built-in customization options, allowing to override default request <see cref="Strategy"/> and <see cref="Priority"/>.</para>
    /// <para>It can also be used to carry arbitrary user-defined <see cref="Properties"/>.</para>
    /// <para>All <see cref="RequestParameters"/> instances are effectively immutable. To build a custom instance, start with <see cref="Empty"/> parameters and use its methods to produce new ones.</para>
    /// </summary>
    [PublicAPI]
    public class RequestParameters
    {
        /// <summary>
        /// Represents an empty <see cref="RequestParameters"/> object. Useful to start building parameters from scratch.
        /// </summary>
        public static readonly RequestParameters Empty = new RequestParameters();

        private readonly ImmutableArrayDictionary<string, object> properties;

        /// <summary>
        /// Create a new instance of <see cref="RequestParameters"/> with specified <paramref name="strategy"/> and <paramref name="priority"/>.
        /// </summary>
        public RequestParameters([CanBeNull] IRequestStrategy strategy = null, [CanBeNull] RequestPriority? priority = null)
            : this(strategy, priority, null, null)
        {
            Strategy = strategy;
            Priority = priority;
        }

        private RequestParameters()
            : this(null, null, null, null)
        {
        }

        private RequestParameters(
            IRequestStrategy strategy,
            RequestPriority? priority,
            ImmutableArrayDictionary<string, object> properties,
            TimeSpan? connectionTimeout)
        {
            Strategy = strategy;
            Priority = priority;
            ConnectionTimeout = connectionTimeout;
            this.properties = properties ?? ImmutableArrayDictionary<string, object>.Empty;
        }

        /// <summary>
        /// <para>An <see cref="IRequestStrategy"/> which will be used to send the request.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if value is <c>null</c>.</para>
        /// <para>See <see cref="Strategies.Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        [CanBeNull]
        public IRequestStrategy Strategy { get; }

        /// <summary>
        /// A <see cref="RequestPriority"/> which will be used to send the request.
        /// </summary>
        public RequestPriority? Priority { get; }

        /// <summary>
        /// A set of additional arbitrary request properties.
        /// </summary>
        [NotNull]
        public IReadOnlyDictionary<string, object> Properties => properties;

        /// <returns>New instance of <see cref="RequestParameters"/> with specified <paramref name="strategy"/>.</returns>
        public RequestParameters WithStrategy([CanBeNull] IRequestStrategy strategy)
            => ReferenceEquals(Strategy, strategy)
                ? this
                : new RequestParameters(strategy, Priority, properties, ConnectionTimeout);

        /// <returns>New instance of <see cref="RequestParameters"/> with specified <paramref name="priority"/>.</returns>
        public RequestParameters WithPriority(RequestPriority? priority)
            => Nullable.Equals(Priority, priority)
                ? this
                : new RequestParameters(Strategy, priority, properties, ConnectionTimeout);

        /// <returns>New instance of <see cref="RequestParameters"/> with specified property.</returns>
        public RequestParameters WithProperty([NotNull] string key, object value)
        {
            var newProperties = properties.Set(key ?? throw new ArgumentNullException(nameof(key)), value);
            return properties == newProperties
                ? this
                : new RequestParameters(Strategy, Priority, newProperties, ConnectionTimeout);
        }

        internal TimeSpan? ConnectionTimeout { get; }

        /// <returns>New instance of <see cref="RequestParameters"/> with specified <paramref name="connectionTimeout"/>.</returns>
        internal RequestParameters WithConnectionTimeout(TimeSpan? connectionTimeout)
            => ConnectionTimeout == connectionTimeout
                ? this
                : new RequestParameters(Strategy, Priority, properties, connectionTimeout);
    }
}
