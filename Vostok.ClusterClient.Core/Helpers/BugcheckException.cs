using System;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal class BugcheckException : Exception
    {
        public BugcheckException(string message)
            : base(message)
        {
        }
    }
}