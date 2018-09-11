using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    public class DefaultHeadersTransform : IRequestTransform, IEnumerable<Header>
    {
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

        [NotNull]
        public Headers DefaultHeaders { get; private set; }

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

        public void Add([NotNull] Header header) =>
            DefaultHeaders = DefaultHeaders.Set(header.Name, header.Value);

        public void Add([NotNull] string name, [NotNull] string value) =>
            DefaultHeaders = DefaultHeaders.Set(name, value);

        public IEnumerator<Header> GetEnumerator() => DefaultHeaders.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}