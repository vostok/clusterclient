using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.Commons.Collections;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represents clusterclient specific parameters of request (strategy, priority and custom properties).
    /// </summary>
    [PublicAPI]
    public class RequestParameters
    {
        public RequestParameters(IRequestStrategy strategy=null, RequestPriority? priority=null)
        {
            Strategy = strategy;
            Priority = priority;
        }

        /// <summary>
        /// Represents an empty <see cref="RequestParameters"/> object. Useful to start building request properties from scratch.
        /// </summary>
        public static readonly RequestParameters Empty = new RequestParameters();

        /// <summary>
        /// <para>A <see cref="Strategy"/> which will be used to send the request.</para>
        /// <para>Uses <see cref="IClusterClientConfiguration.DefaultRequestStrategy"/> if value is <c>null</c>.</para>
        /// <para>See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.</para>
        /// </summary>
        public IRequestStrategy Strategy { get; }
        
        /// <summary>
        /// A <see cref="RequestPriority"/> which will be used to send the request.
        /// </summary>
        public RequestPriority? Priority { get; }

        /// <summary>
        /// A set of additional request properties.
        /// </summary>
        [PublicAPI]
        public IReadOnlyDictionary<string, object> Properties => properties;
        
        private readonly ImmutableArrayDictionary<string, object> properties = new ImmutableArrayDictionary<string, object>();

        private RequestParameters(){}
        
        private RequestParameters(
            IRequestStrategy strategy,
            RequestPriority? priority,
            ImmutableArrayDictionary<string, object> properties)
        {
            Strategy = strategy;
            Priority = priority;
            this.properties = properties ?? ImmutableArrayDictionary<string, object>.Empty;
        }

        /// <returns>New instance of <see cref="RequestParameters"/> with specified <paramref name="strategy"/>.</returns>
        [PublicAPI]
        public RequestParameters WithStrategy(IRequestStrategy strategy)
            => new RequestParameters(strategy, Priority, properties);
    
        /// <returns>New instance of <see cref="RequestParameters"/> with specified <paramref name="priority"/>.</returns>
        [PublicAPI]
        public RequestParameters WithPriority(RequestPriority? priority)
            => new RequestParameters(Strategy, priority, properties);
        
        
        /// <returns>New instance of <see cref="RequestParameters"/> with specified property.</returns>
        [PublicAPI]
        public RequestParameters WithProperty(string key, object value)
            => new RequestParameters(Strategy, Priority, properties.Set(key, value));
    }
}