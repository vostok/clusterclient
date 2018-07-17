using System.Text;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Transport.Webrequest
{
    // (iloktionov): A dirty hack that exploits header serialization implementation inside HttpWebRequest
    // (iloktionov): (it just directly casts every character to byte, so we can cook a longer
    // (iloktionov): ASCII string from initial UTF-8 string to obtain correct byte representation).
    internal static class NonAsciiHeadersFixer
    {
        private const char MinASCII = '\u0020';
        private const char MaxASCII = '\u007E';

        public static Request Fix(Request request)
        {
            var headers = request.Headers;
            if (headers == null)
                return request;

            foreach (var header in headers)
            {
                if (IsAscii(header.Value))
                    continue;

                headers = headers.Set(header.Name, FixNonAscii(header.Value));
            }

            if (!ReferenceEquals(headers, request.Headers))
                request = request.WithHeaders(headers);

            return request;
        }

        private static string FixNonAscii(string value)
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(value);

            var fixedStringBuilder = new StringBuilder(utf8Bytes.Length);

            foreach (var t in utf8Bytes)
                fixedStringBuilder.Append((char)t);

            return fixedStringBuilder.ToString();
        }

        private static bool IsAscii(string value)
        {
            foreach (var ch in value)
                if (ch < MinASCII || ch > MaxASCII)
                    return false;

            return true;
        }
    }
}