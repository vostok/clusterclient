using System.IO;
using JetBrains.Annotations;

namespace Vostok.Clusterclient.Core.Model
{
    /// <summary>
    /// Represents a request body with an underlying <see cref="System.IO.Stream"/> used in <see cref="Request"/>.
    /// </summary>
    [PublicAPI]
    public interface IStreamContent
    {
        /// <summary>
        /// A stream which contains request body.
        /// </summary>
        Stream Stream { get; }

        /// <summary>
        /// A response body length. If value is provided, it will be used for <see cref="HeaderNames.ContentLength"/> header.
        /// </summary>
        long? Length { get; }
    }
}