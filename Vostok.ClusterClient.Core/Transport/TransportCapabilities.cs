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
        /// Empty TransportCapabilities
        /// </summary>
        None = 0,
        /// <summary>
        /// Transport is capable to stream request content from <see cref="Request.StreamContent"/>.
        /// </summary>
        RequestStreaming = 1 << 0,
        /// <summary>
        /// Transport is capable to stream response content to <see cref="Response.Stream"/>
        /// </summary>
        ResponseStreaming = 1 << 1
    }
}