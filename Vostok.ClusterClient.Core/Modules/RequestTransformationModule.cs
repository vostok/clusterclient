using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transforms;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class RequestTransformationModule : IRequestModule
    {
        private readonly IList<IRequestTransformMetadata> transforms;

        public RequestTransformationModule(IList<IRequestTransformMetadata> transforms)
        {
            this.transforms = transforms;
        }

        public async Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (transforms != null && transforms.Count > 0)
            {
                foreach (var transform in transforms)
                {
                    switch (transform)
                    {
                        case IAsyncRequestTransform asyncRequestTransform:
                            context.Request = await asyncRequestTransform
                                .TransformAsync(context.Request)
                                .ConfigureAwait(false);
                            break;

                        case IRequestTransform requestTransform:
                            context.Request = requestTransform.Transform(context.Request);
                            break;
                    }
                }
            }

            SubstituteStreamContent(context);
            return await next(context).ConfigureAwait(false);
        }

        private static void SubstituteStreamContent(IRequestContext context)
        {
            var streamContent = context.Request.StreamContent;
            if (streamContent != null)
                context.Request = context.Request.WithContent(new SingleUseStreamContent(streamContent.Stream, streamContent.Length));
        }
    }
}