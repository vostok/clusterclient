using System.Diagnostics;
using System.Reflection;
using Vostok.ClusterClient.Core.Utilities;

namespace Vostok.ClusterClient.Core.Net
{
    internal static class HttpClientIdentity
    {
        private static string identity;
        private static volatile bool init;

        public static string Get()
        {
            if (!init)
                Initialize();
            return identity;
        }

        private static void Initialize()
        {
            identity = GetIdentity();
            init = true;
        }
        
        private static string GetIdentity()
        {
            try
            {
                if (RuntimeDetector.IsDotNetCore)
                    return GetEntryAssemblyNameOrNull();

                return GetProcessNameOrNull() ?? GetEntryAssemblyNameOrNull();
            }
            catch
            {
                return null;
            }
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
        
        private static string GetEntryAssemblyNameOrNull()
        {
            try
            {
                return Assembly.GetEntryAssembly().GetName().Name;
            }
            catch
            {
                return null;
            }
        }
    }
}