using System;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// Represents a response transform which uses external delegate to modify responses.
    /// </summary>
    public class AdHocResponseTransform : IResponseTransform
    {
        private readonly Func<Response, Response> transform;

        /// <summary>
        /// Initializes a new instance of <see cref="AdHocResponseTransform"/> class.
        /// </summary>
        /// <param name="transform">An external delegate which will be used to modify responses.</param>
        public AdHocResponseTransform(Func<Response, Response> transform)
        {
            this.transform = transform;
        }

        /// <inheritdoc />
        public Response Transform(Response request) => transform(request);
    }
}