namespace Vostok.ClusterClient.Core.Model
{
    internal class StreamAlreadyUsedException : ClusterClientException
    {
        public StreamAlreadyUsedException(string message)
            : base(message)
        {
        }
    }
}