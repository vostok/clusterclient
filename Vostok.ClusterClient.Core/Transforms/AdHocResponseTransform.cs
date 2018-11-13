using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Transforms
{
    /// <summary>
    /// Represents a response transform which uses external delegate to modify responses.
    /// </summary>
    [PublicAPI]
    public class AdHocResponseTransform : IResponseTransform
    {
        private readonly Func<Response, Response> transform;

        /// <param name="transform">An external delegate which will be used to modify responses.</param>
        public AdHocResponseTransform(Func<Response, Response> transform)
        {
            this.transform = transform;
        }

        /// <inheritdoc />
        public Response Transform(Response request) => transform(request);
    }
}