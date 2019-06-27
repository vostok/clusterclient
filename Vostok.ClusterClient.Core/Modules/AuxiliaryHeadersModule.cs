using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Modules
{
    internal class AuxiliaryHeadersModule : IRequestModule
    {
        private readonly string priorityHeader;
        private readonly string identityHeader;

        public AuxiliaryHeadersModule(
            [NotNull] string priorityHeader = HeaderNames.RequestPriority, 
            [NotNull] string identityHeader = HeaderNames.ApplicationIdentity)
        {
            this.priorityHeader = priorityHeader ?? throw new ArgumentNullException(nameof(priorityHeader));
            this.identityHeader = identityHeader ?? throw new ArgumentNullException(nameof(identityHeader));
        }

        public Task<ClusterResult> ExecuteAsync(IRequestContext context, Func<IRequestContext, Task<ClusterResult>> next)
        {
            var priority = context.Parameters.Priority;
            if (priority.HasValue)
                context.Request = context.Request.WithHeader(priorityHeader, priority.Value);

            var applicationName = context.ClientApplicationName;
            if (!string.IsNullOrEmpty(applicationName) && context.Request.Headers?[identityHeader] == null)
                context.Request = context.Request.WithHeader(identityHeader, applicationName);

            return next(context);
        }
    }
}