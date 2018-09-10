using System;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Transforms;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// Represents a response transform which uses external delegate to modify requests.
    /// </summary>
    public class AdHocResponseTransform : IResponseTransform
    {
        private readonly Func<Response, Response> transform;

        public AdHocResponseTransform(Func<Response, Response> transform)
        {
            this.transform = transform;
        }

        public Response Transform(Response request) => transform(request);
    }
}