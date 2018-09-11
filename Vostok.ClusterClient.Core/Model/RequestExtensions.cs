using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.ClusterClient.Core.Model
{
    public static class RequestExtensions
    {
        /// <summary>
        /// Returns all validation errors for this <see cref="Request"/> instance. An empty sequence is returned for a valid request.
        /// </summary>
        public static IEnumerable<string> Validate(this Request request, bool validateHttpMethod = true)
        {
            if (validateHttpMethod && !RequestMethods.All.Contains(request.Method))
                yield return $"Request method has unsupported value '{request.Method}'.";

            if (request.Url.IsAbsoluteUri)
            {
                var scheme = request.Url.Scheme;
                if (scheme != Uri.UriSchemeHttp && scheme != Uri.UriSchemeHttps)
                    yield return $"Request url has unsupported scheme '{scheme}'. Only http and https schemes are allowed.";
            }

            if (request.HasBody && (request.Method == RequestMethods.Get || request.Method == RequestMethods.Head))
                yield return $"Sending a body is not allowed with {request.Method} requests.";
        }
                
        /// <summary>
        /// <para>Returns true if current <see cref="Request"/> instance is valid, or false otherwise.</para>
        /// <para><see cref="Validate"/> method can be used to obtain error messages.</para>
        /// </summary>
        public static bool IsValid(this Request request) => !request.Validate().Any();

        internal static bool IsValidCustomizable(this Request request, bool validateHttpMethod)
        {
            return !request.Validate(validateHttpMethod).Any();
        }
    }
}