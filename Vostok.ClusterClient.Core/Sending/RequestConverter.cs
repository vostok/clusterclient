using System;
using System.Collections.Generic;
using Vostok.ClusterClient.Core.Model;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Sending
{
    internal class RequestConverter : IRequestConverter
    {
        private readonly ILog log;
        private readonly bool deduplicateSegments;

        public RequestConverter(ILog log, bool deduplicateSegments)
        {
            this.log = log;
            this.deduplicateSegments = deduplicateSegments;
        }

        public Request TryConvertToAbsolute(Request relativeRequest, Uri replica)
        {
            if (!replica.IsAbsoluteUri)
            {
                LogReplicaUrlNotAbsolute(replica);
                return null;
            }

            if (!string.IsNullOrEmpty(replica.Query))
            {
                LogReplicaUrlContainsQuery(replica);
                return null;
            }

            var requestUrl = relativeRequest.Url;
            if (requestUrl.IsAbsoluteUri)
            {
                LogRequestHasAbsoluteUrl(requestUrl);
                return null;
            }

            try
            {
                var convertedUrl = ConvertUrl(requestUrl, replica);
                var convertedRequest = relativeRequest.WithUrl(convertedUrl);

                return convertedRequest;
            }
            catch (Exception error)
            {
                LogUrlConversionException(replica, requestUrl, error);
                return null;
            }
        }

        private static void DeduplicateSegments(string replicaPath, ref string requestUrl)
        {
            var lastMatchingSegment = null as Segment?;
            var seenAllReplicaSegments = false;

            using (var replicaSegments = EnumerateSegments(replicaPath))
            using (var requestSegments = EnumerateSegments(requestUrl))
            {
                if (!requestSegments.MoveNext())
                    return;

                while (true)
                {
                    if (!replicaSegments.MoveNext())
                    {
                        seenAllReplicaSegments = true;
                        break;
                    }

                    if (replicaSegments.Current.Equals(requestSegments.Current))
                    {
                        lastMatchingSegment = requestSegments.Current;

                        if (requestSegments.MoveNext())
                            continue;

                        if (!replicaSegments.MoveNext())
                            seenAllReplicaSegments = true;

                        break;
                    }

                    if (lastMatchingSegment.HasValue)
                        return;
                }
            }

            if (seenAllReplicaSegments && lastMatchingSegment.HasValue)
                requestUrl = requestUrl.Substring(lastMatchingSegment.Value.Offset + lastMatchingSegment.Value.Length);
        }

        private static IEnumerator<Segment> EnumerateSegments(string url)
        {
            var segmentBeginning = 0;
            var pathLength = 0;

            for (var i = 0; i < url.Length; i++)
            {
                var current = url[i];
                if (current == '?')
                    break;

                if (current == '/')
                {
                    if (i > segmentBeginning)
                        yield return new Segment(url, segmentBeginning, i - segmentBeginning);

                    segmentBeginning = i + 1;
                }

                pathLength++;
            }

            if (segmentBeginning < pathLength)
                yield return new Segment(url, segmentBeginning, pathLength - segmentBeginning);
        }

        private Uri ConvertUrl(Uri requestUrl, Uri replica)
        {
            var baseUrl = replica.OriginalString;
            var baseUrlEndsWithSlash = baseUrl.EndsWith("/");

            var appendedUrl = requestUrl.OriginalString;

            if (deduplicateSegments)
                DeduplicateSegments(replica.AbsolutePath, ref appendedUrl);

            var appendedUrlStartsWithSlash = appendedUrl.StartsWith("/");

            if (baseUrlEndsWithSlash && appendedUrlStartsWithSlash)
                appendedUrl = appendedUrl.Substring(1);

            if (!baseUrlEndsWithSlash && !appendedUrlStartsWithSlash && !string.IsNullOrEmpty(appendedUrl))
                // (deniaa): If appendedUrl is empty, we should not add slash to the end of the Uri. In RFC Uri's with a slash on the end are not equals to Uris without it.
                appendedUrl = "/" + appendedUrl;

            return new Uri(baseUrl + appendedUrl, UriKind.Absolute);
        }

        #region Segment

        private struct Segment
        {
            public readonly int Offset;
            public readonly int Length;
            private readonly string origin;

            public Segment(string origin, int offset, int length)
            {
                this.origin = origin;
                Offset = offset;
                Length = length;
            }

            public bool Equals(Segment other)
            {
                if (Length != other.Length)
                    return false;

                for (var i = 0; i < Length; i++)
                    if (this[i] != other[i])
                        return false;

                return true;
            }

            private char this[int index] => origin[Offset + index];
        }

        #endregion

        #region Logging 

        private void LogReplicaUrlNotAbsolute(Uri replica) =>
            log.Error($"Given replica url is not absolute: '{replica}'. Absolute url is expected here.");

        private void LogReplicaUrlContainsQuery(Uri replica) =>
            log.Error($"Replica url contains query parameters: '{replica}'. No query parameters are allowed for replicas.");

        private void LogRequestHasAbsoluteUrl(Uri requestUrl) =>
            log.Error($"Request contains absolute url: '{requestUrl}'. Relative url is expected instead.");

        private void LogUrlConversionException(Uri replica, Uri requestUrl, Exception error) =>
            log.Error(error, $"Failed to merge replica url '{replica}' and request url '{requestUrl}'.");

        #endregion
    }
}