using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Strategies;
using Vostok.Commons.Collections;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represents a clusterclient specific parameters of request (strategy, priority and custom properties).
    /// </summary>
    [PublicAPI]
    public class RequestParameters
    {
        /// <summary>
        /// Represents an empty <see cref="RequestParameters"/> object. Useful to start building request properties from scratch.
        /// </summary>
        [PublicAPI]
        public static readonly RequestParameters Empty = new RequestParameters();
        
        /// <summary>
        /// See <see cref="Strategy"/> class for some prebuilt strategies and convenient factory methods.
        /// </summary>
        [PublicAPI]
        public IRequestStrategy Strategy { get; }
        
        [PublicAPI]
        public RequestPriority? Priority { get; }

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

        [PublicAPI]
        public RequestParameters WithStrategy(IRequestStrategy strategy)
            => new RequestParameters(strategy, Priority, properties);
    
        [PublicAPI]
        public RequestParameters WithPriority(RequestPriority? priority)
            => new RequestParameters(Strategy, priority, properties);
        
        [PublicAPI]
        public RequestParameters WithCustomProperty(string key, object value)
            => new RequestParameters(Strategy, Priority, properties.Set(key, value));
    }
}