namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal enum HttpActionStatus
    {
        Success,
        ConnectionFailure,
        SendFailure,
        ReceiveFailure,
        Timeout,
        RequestCanceled,
        ProtocolError,
        UnknownFailure,
        InsufficientStorage,
        UserStreamFailure
    }
}