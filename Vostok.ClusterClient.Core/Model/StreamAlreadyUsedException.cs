using System;

namespace Vostok.ClusterClient.Core.Model
{
    public class StreamAlreadyUsedException : Exception
    {
        public StreamAlreadyUsedException(string message)
            : base(message)
        {
        }
    }
}