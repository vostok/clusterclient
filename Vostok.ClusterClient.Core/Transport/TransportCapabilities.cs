using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Transport
{
    /// <summary>
    /// Represents a set of optional capabilities one can expect to find in transport implementations.
    /// </summary>
    [PublicAPI]
    [Flags]
    public enum TransportCapabilities
    {
        /// <summary>
        /// Transport has no special capabilites.
        /// </summary>
        None = 0,

        /// <summary>
        /// Transport is capable of streaming request content from <see cref="Request.StreamContent"/>.
        /// </summary>
        RequestStreaming = 1 << 0,

        /// <summary>
        /// Transport is capable of streaming response content to <see cref="Response.Stream"/>
        /// </summary>
        ResponseStreaming = 1 << 1,

        /// <summary>
        /// Transport is capable of sending <see cref="CompositeContent"/> as request body.
        /// </summary>
        RequestCompositeBody = 1 << 2
    }
}