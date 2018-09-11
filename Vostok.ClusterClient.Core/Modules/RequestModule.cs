namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// <para>Defines request modules from standard request pipeline.</para>
    /// </summary>
    public enum RequestModule
    {
        LeakPrevention,
        GlobalErrorCatching,
        RequestTransformation,
        RequestPriority,
        Logging,
        ResponseTransformation,
        SendingErrorCatching,
        RequestValidation,
        TimeoutValidation,
        RequestRetry,
        AdaptiveThrottling,
        ReplicaBudgeting,
        AbsoluteUrlSender,
        RequestExecution
    }
}