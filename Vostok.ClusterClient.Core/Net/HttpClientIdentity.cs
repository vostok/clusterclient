using System.Diagnostics;
using Vostok.Commons.Utilities;

namespace Vostok.ClusterClient.Core.Net
{
    public static class HttpClientIdentity
    {
        private static readonly string Identity;

        static HttpClientIdentity()
        {
            Identity = GetIdentityFromHostingEnvironmentOrNull();
            if (!IsValidIdentity(Identity))
                Identity = GetProcessNameOrNull();

            if (!IsValidIdentity(Identity))
                Identity = "Unknown";

            Identity = Identity.Replace('/', '.');
        }

        public static string Get() => Identity;

        private static bool IsValidIdentity(string identity) => !string.IsNullOrWhiteSpace(identity);

        private static string GetIdentityFromHostingEnvironmentOrNull()
        {
            try
            {
                if (RuntimeDetector.IsDotNetFramework)
                    return GetIdentityForDotNetFramework();

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static string GetIdentityForDotNetFramework()
        {
            // todo(Mansiper): fix it
            return null;
            /*try
            {
                if (!HostingEnvironment.IsHosted)
                    return null;

                var vPath = HostingEnvironment.ApplicationVirtualPath;
                var siteName = HostingEnvironment.SiteName;
                if (vPath == null || vPath == "/")
                    return siteName;

                return siteName + vPath;
            }
            catch
            {
                return null;
            }*/
        }

        private static string GetProcessNameOrNull()
        {
            try
            {
                return Process.GetCurrentProcess().ProcessName;
            }
            catch
            {
                return null;
            }
        }
    }
}