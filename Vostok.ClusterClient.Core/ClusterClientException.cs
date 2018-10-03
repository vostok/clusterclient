using System;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core
{
    /// <summary>
    /// An exception that represents ClusterClient error. Should be thrown by ClusterClient and extension modules (like transports, ...) only.
    /// </summary>
    [PublicAPI]
    public class ClusterClientException : Exception
    {
        /// <inheritdoc />
        public ClusterClientException()
        {
        }

        /// <inheritdoc />
        public ClusterClientException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        public ClusterClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}