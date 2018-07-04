using System;

namespace Vostok.ClusterClient.Core
{
    public class ClusterClientException : Exception
    {
        public ClusterClientException()
        {
        }

        public ClusterClientException(string message)
            : base(message)
        {
        }

        public ClusterClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}