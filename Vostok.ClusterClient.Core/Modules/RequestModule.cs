using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// <para>Defines request modules from standard request pipeline.</para>
    /// </summary>
    [PublicAPI]
    public enum RequestModule
    {
        Default,
        LeakPrevention,
        GlobalErrorHandling,
        RequestTransformation,
        Priority,
        Logging,
        ResponseTransformation,
        RequestErrorHandling,
        RequestValidation,
        TimeoutValidation,
        Retry,
        Sending,
        Execution
    }
}