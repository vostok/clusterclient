using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Commons.Collections;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// Represents clusterclient specific parameters of request (strategy, priority and custom properties).
    /// </summary>
    [PublicAPI]
    public class RequestParameters
    {
        /// <summary>
        /// Represents an empty <see cref="RequestParameters"/> object. Useful to start building request properties from scratch.
        /// </summary>
        public static readonly RequestParameters Empty = new RequestParameters();

        private readonly ImmutableArrayDictionary<string, object> properties = new ImmutableArrayDictionary<string, object>();

        /// <summary>
        /// Create a new instance of <see cref="RequestParameters"/> with specified <paramref name="strategy"/> and <paramref name="priority"/>.
        /// </summary>
        public RequestParameters([CanBeNull] IRequestStrategy strategy = null, [CanBeNull] RequestPriority? priority = null)
        {
            Strategy = strategy;
            Priority = priority;
        }

        private RequestParameters()
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
        /// <para>A <see cref="Strategy"/> which will be used to send the request.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if value is <c>null</c>.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        [CanBeNull]
        public IRequestStrategy Strategy { get; }

        /// <summary>
        /// A <see cref="RequestPriority"/> which will be used to send the request.
        /// </summary>
        public RequestPriority? Priority { get; }

        /// <summary>
        /// A set of additional request properties.
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