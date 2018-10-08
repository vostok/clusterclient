using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transforms;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class RequestTransformationModule : IRequestModule
    {
        private readonly IList<IRequestTransform> transforms;

        public RequestTransformationModule(IList<IRequestTransform> transforms)
        {
            this.transforms = transforms;
        }

        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (transforms != null && transforms.Count > 0)
                foreach (var transform in transforms)
                    context.Request = transform.Transform(context.Request);

            SubstituteStreamContent(context);

            return next(context);
        }

        private static void SubstituteStreamContent(IRequestContext context)
        {
            var streamContent = context.Request.StreamContent;
            if (streamContent != null)
                context.Request = context.Request.WithContent(new SingleUseStreamContent(streamContent.Stream, streamContent.Length));
        }
    }
}