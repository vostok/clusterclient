using System.IO;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    internal static class ResponseFactory
    {
        public static Response BuildSuccessResponse(WebRequestState state) =>
            BuildResponse((ResponseCode)(int)state.Response.StatusCode, state);

        public static Response BuildFailureResponse(HttpActionStatus status, WebRequestState state)
        {
            switch (status)
            {
                case HttpActionStatus.SendFailure:
                    return BuildResponse(ResponseCode.SendFailure, state);

                case HttpActionStatus.ReceiveFailure:
                    return BuildResponse(ResponseCode.ReceiveFailure, state);

                case HttpActionStatus.Timeout:
                    return BuildResponse(ResponseCode.RequestTimeout, state);

                case HttpActionStatus.RequestCanceled:
                    return BuildResponse(ResponseCode.Canceled, state);

                case HttpActionStatus.InsufficientStorage:
                    return BuildResponse(ResponseCode.InsufficientStorage, state);

                case HttpActionStatus.UserStreamFailure:
                    return BuildResponse(ResponseCode.StreamInputFailure, state);

                default:
                    return BuildResponse(ResponseCode.UnknownFailure, state);
            }
        }

        public static Response BuildResponse(ResponseCode code, WebRequestState state) =>
            new Response(
                code,
                CreateResponseContent(state),
                CreateResponseHeaders(state),
                CreateResponseStream(state)
            );

        private static Content CreateResponseContent(WebRequestState state)
        {
            if (state.ReturnStreamDirectly)
                return null;

            if (state.BodyBuffer != null)
                return new Content(state.BodyBuffer, 0, state.BodyBufferLength);

            if (state.BodyStream != null)
                return new Content(state.BodyStream.GetBuffer(), 0, (int)state.BodyStream.Position);

            return null;
        }

        private static Headers CreateResponseHeaders(WebRequestState state)
        {
            var headers = Headers.Empty;

            if (state.Response == null)
                return headers;

            foreach (var key in state.Response.Headers.AllKeys)
                headers = headers.Set(key, state.Response.Headers[key]);

            return headers;
        }

        private static Stream CreateResponseStream(WebRequestState state) =>
            state.ReturnStreamDirectly ? new ResponseBodyStream(state) : null;
    }
}