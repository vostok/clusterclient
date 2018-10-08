using System;

namespace Vostok.ClusterClient.Core.Modules
{
    internal static class RequestModulesMapping
    {
        public static Type GetModuleType(RequestModule module)
        {
            switch (module)
            {
                case RequestModule.LeakPrevention:
                    return typeof(LeakPreventionModule);
                case RequestModule.GlobalErrorCatching:
                    return typeof(GlobalErrorCatchingModule);
                case RequestModule.RequestTransformation:
                    return typeof(RequestTransformationModule);
                case RequestModule.RequestPriority:
                    return typeof(RequestPriorityModule);
                case RequestModule.ApplicationName:
                    return typeof(ClientApplicationIdentityModule);
                case RequestModule.Logging:
                    return typeof(LoggingModule);
                case RequestModule.ResponseTransformation:
                    return typeof(ResponseTransformationModule);
                case RequestModule.ErrorCatching:
                    return typeof(ErrorCatchingModule);
                case RequestModule.RequestValidation:
                    return typeof(RequestValidationModule);
                case RequestModule.TimeoutValidation:
                    return typeof(TimeoutValidationModule);
                case RequestModule.RequestRetry:
                    return typeof(RequestRetryModule);
                case RequestModule.AbsoluteUrlSender:
                    return typeof(AbsoluteUrlSenderModule);
                case RequestModule.RequestExecution:
                    return typeof(RequestExecutionModule);
                case RequestModule.AdaptiveThrottling:
                    return typeof(AdaptiveThrottlingModule);
                case RequestModule.ReplicaBudgeting:
                    return typeof(ReplicaBudgetingModule);
                default:
                    throw new ArgumentOutOfRangeException(nameof(module), module, $"Unexpected {nameof(RequestModule)} value.");
            }
        }
    }
}