namespace Vostok.ClusterClient.Core.Model
{
    internal static class RequestMiscExtensions
    {
        internal static bool ContainsAlreadyUsedStream(this Request request)
        {
            return request.StreamContent is SingleUseStreamContent streamContent && streamContent.WasUsed;
        }
    }
}