using System;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class AuxiliaryHeadersModule : IRequestModule
    {
        private readonly RequestPriority? defaultPriority;

        public AuxiliaryHeadersModule(RequestPriority? defaultPriority)
        {
            this.defaultPriority = defaultPriority;
        }

        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            if (!context.Parameters.Priority.HasValue && defaultPriority.HasValue)
                context.Parameters = context.Parameters.WithPriority(defaultPriority);
            
            var priority = context.Parameters.Priority;
            if (priority.HasValue)
                context.Request = context.Request.WithHeader(HeaderNames.RequestPriority, priority.Value);

            if (!string.IsNullOrEmpty(context.ClientApplicationName))
                context.Request = context.Request.WithHeader(HeaderNames.ApplicationIdentity, context.ClientApplicationName);

            return next(context);
        }
    }
}