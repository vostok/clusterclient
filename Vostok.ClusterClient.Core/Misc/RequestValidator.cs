using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Misc
{
    internal static class RequestValidator
    {
        /// <summary>
        /// Returns all validation errors for this <see cref="Request"/> instance. An empty sequence is returned for a valid request.
        /// </summary>
        public static IEnumerable<string> Validate(Request request)
        {
            if (request.Url.IsAbsoluteUri)
            {
                var scheme = request.Url.Scheme;
                if (scheme != Uri.UriSchemeHttp && scheme != Uri.UriSchemeHttps)
                    yield return $"Request url has unsupported scheme '{scheme}'. Only http and https schemes are allowed.";
            }
        }

        /// <summary>
        /// <para>Returns true if current <see cref="Request"/> instance is valid, or false otherwise.</para>
        /// <para><see cref="Validate"/> method can be used to obtain error messages.</para>
        /// </summary>
        public static bool IsValid(Request request) => !Validate(request).Any();
    }
}