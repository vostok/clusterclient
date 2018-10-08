using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class RequestPriorityModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var priority = context.Parameters.Priority;
            if (priority.HasValue)
                context.Request = context.Request.WithHeader(HeaderNames.RequestPriority, priority.Value);

            return next(context);
        }
    }
}