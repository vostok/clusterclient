using System.IO;
using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represents a request body with an underlying <see cref="System.IO.Stream"/> used in <see cref="Request"/>.
    /// </summary>
    [PublicAPI]
    public interface IStreamContent
    {
        Stream Stream { get; }

        long? Length { get; }
    }
}