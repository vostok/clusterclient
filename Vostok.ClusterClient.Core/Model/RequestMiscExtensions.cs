namespace Vostok.ClusterClient.Core.Model
{
    internal static class RequestMiscExtensions
    {
        internal static bool ContainsAlreadyUsedStream(this Request request) =>
            request.StreamContent is SingleUseStreamContent streamContent && streamContent.WasUsed;
    }
}