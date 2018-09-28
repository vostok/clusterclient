using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Vostok.ClusterClient.Core.Utilities;

namespace Vostok.ClusterClient.Core.Net
{
    internal static class ApplicationIdentity
    {
        private static readonly Lazy<string> Identity = new Lazy<string>(GetIdentity, LazyThreadSafetyMode.PublicationOnly);

        public static string Get()
        {
            return Identity.Value;
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