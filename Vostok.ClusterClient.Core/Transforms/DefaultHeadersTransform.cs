using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// A <see cref="IRequestTransform"/> which append default headers to <see cref="Request"/>.
    /// </summary>
    [PublicAPI]
    public class DefaultHeadersTransform : IRequestTransform, IEnumerable<Header>
    {
        /// <summary>
        /// Creates new instance of <see cref="DefaultHeadersTransform"/>.
        /// </summary>
        /// <param name="defaultHeaders">A set of default headers which will be appended to requests on transform.</param>
        public DefaultHeadersTransform([CanBeNull] IEnumerable<Header> defaultHeaders = null)
        {
            DefaultHeaders = Headers.Empty;
            if (defaultHeaders == null)
                return;
            
            foreach (var header in defaultHeaders)
            {
                DefaultHeaders = DefaultHeaders.Set(header.Name, header.Value);
            }
        }

        /// <summary>
        /// A set of default headers.
        /// </summary>
        [NotNull]
        public Headers DefaultHeaders { get; private set; }

        /// <inheritdoc />
        public Request Transform(Request request)
        {
            if (DefaultHeaders.Count == 0)
                return request;

            var newHeaders = DefaultHeaders;

            if (request.Headers?.Count > 0)
                foreach (var header in request.Headers)
                    newHeaders = newHeaders.Set(header.Name, header.Value);

            return request.WithHeaders(newHeaders);
        }

        /// <summary>
        /// Add <paramref name="header"/> to <see cref="DefaultHeaders"/>.
        /// </summary>
        public void Add([NotNull] Header header) =>
            DefaultHeaders = DefaultHeaders.Set(header.Name, header.Value);

        /// <summary>
        /// Add header with name <paramref name="name"/> and value <paramref name="value"/> to <see cref="DefaultHeaders"/>.
        /// </summary>
        public void Add([NotNull] string name, [NotNull] string value) =>
            DefaultHeaders = DefaultHeaders.Set(name, value);

        /// <inheritdoc />
        public IEnumerator<Header> GetEnumerator() => DefaultHeaders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}