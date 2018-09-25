using System;
using System.Threading.Tasks;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Modules
{
    internal class RequestPriorityModule : IRequestModule
    {
        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            //TODO: priority from context
            var priority = context.Priority;
            //TODO: set priority in headers
            if (priority.HasValue)
                context.Request = context.Request.WithHeader(HeaderNames.RequestPriority, priority.Value);

            return next(context);
        }
    }
}