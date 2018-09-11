namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Provides cached <see cref="Response"/> instances for common <see cref="ResponseCode">ResponseCodes</see>."
    /// </summary>
    public static class Responses
    {
        // (iloktionov): 0xx
        public static readonly Response Unknown = new Response(ResponseCode.Unknown);

        // (iloktionov): 2xx
        public static readonly Response Ok = new Response(ResponseCode.Ok);
        public static readonly Response Created = new Response(ResponseCode.Created);
        public static readonly Response Accepted = new Response(ResponseCode.Accepted);
        public static readonly Response NoContent = new Response(ResponseCode.NoContent);
        public static readonly Response PartialContent = new Response(ResponseCode.PartialContent);

        // (iloktionov): 3xx
        public static readonly Response MovedPermanently = new Response(ResponseCode.MovedPermanently);
        public static readonly Response NotModified = new Response(ResponseCode.NotModified);

        // (iloktionov): 4xx
        public static readonly Response BadRequest = new Response(ResponseCode.BadRequest);
        public static readonly Response Unauthorized = new Response(ResponseCode.Unauthorized);
        public static readonly Response Forbidden = new Response(ResponseCode.Forbidden);
        public static readonly Response NotFound = new Response(ResponseCode.NotFound);
        public static readonly Response MethodNotAllowed = new Response(ResponseCode.MethodNotAllowed);
        public static readonly Response Timeout = new Response(ResponseCode.RequestTimeout);
        public static readonly Response Conflict = new Response(ResponseCode.Conflict);
        public static readonly Response Gone = new Response(ResponseCode.Gone);
        public static readonly Response Throttled = new Response(ResponseCode.TooManyRequests);
        public static readonly Response UnknownFailure = new Response(ResponseCode.UnknownFailure);
        public static readonly Response Canceled = new Response(ResponseCode.Canceled);
        public static readonly Response StreamReuseFailure = new Response(ResponseCode.StreamReuseFailure);
        public static readonly Response StreamInputFailure = new Response(ResponseCode.StreamInputFailure);

        // (iloktionov): 5xx
        public static readonly Response InternalServerError = new Response(ResponseCode.InternalServerError);
        public static readonly Response NotImplemented = new Response(ResponseCode.NotImplemented);
        public static readonly Response BadGateway = new Response(ResponseCode.BadGateway);
        public static readonly Response ServiceUnavailable = new Response(ResponseCode.ServiceUnavailable);
        public static readonly Response ProxyTimeout = new Response(ResponseCode.ProxyTimeout);
    }
}
