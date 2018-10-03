using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// Represents a request transform which uses external delegate to modify requests.
    /// </summary>
    [PublicAPI]
    public class AdHocRequestTransform : IRequestTransform
    {
        private readonly Func<Request, Request> transform;

        /// <param name="transform">An external delegate which will be used to modify requests.</param>
        public AdHocRequestTransform(Func<Request, Request> transform)
        {
            this.transform = transform;
        }

        /// <inheritdoc />
        public Request Transform(Request request) => transform(request);
    }
}