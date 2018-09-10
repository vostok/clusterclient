using System;
using Vostok.ClusterClient.Abstractions.Model;
using Vostok.ClusterClient.Abstractions.Transforms;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Transforms
{
    /// <summary>
    /// Represents a request transform which uses external delegate to modify requests.
    /// </summary>
    public class AdHocRequestTransform : IRequestTransform
    {
        private readonly Func<Request, Request> transform;

        public AdHocRequestTransform(Func<Request, Request> transform)
        {
            this.transform = transform;
        }

        public Request Transform(Request request) => transform(request);
    }
}