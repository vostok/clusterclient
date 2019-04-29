using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Transforms
{
    /// <summary>
    /// Represents an async request transform that uses an external delegate to modify requests.
    /// </summary>
    [PublicAPI]
    public class AdHocAsyncRequestTransform : IAsyncRequestTransform
    {
        private readonly Func<Request, Task<Request>> transform;

        /// <param name="transform">An external delegate which will be used to modify requests.</param>
        public AdHocAsyncRequestTransform(Func<Request, Task<Request>> transform)
        {
            this.transform = transform;
        }

        /// <inheritdoc />
        public Task<Request> TransformAsync(Request request) => transform(request);
    }
}