using System.IO;
using System.Threading;
using Vostok.ClusterClient.Abstractions.Model;

namespace Vostok.ClusterClient.Core.Model
{
    internal class SingleUseStreamContent : IStreamContent
    {
        private Stream stream;

        public SingleUseStreamContent(Stream stream, long? length)
        {
            this.stream = stream;

            Length = length;
        }

        public Stream Stream
        {
            get
            {
                var streamToReturn = Interlocked.Exchange(ref stream, null);
                if (streamToReturn == null)
                    throw new StreamAlreadyUsedException("Detected an attempt to use request body stream twice, which is not allowed.");

                return streamToReturn;
            }
        }

        public bool WasUsed => stream == null;

        public long? Length { get; }
    }
}