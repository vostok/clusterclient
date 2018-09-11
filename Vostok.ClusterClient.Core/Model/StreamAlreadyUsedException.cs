using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    [PublicAPI]
    public class StreamAlreadyUsedException : ClusterClientException
    {
        public StreamAlreadyUsedException(string message)
            : base(message)
        {
        }
    }
}