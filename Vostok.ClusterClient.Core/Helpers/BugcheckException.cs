using System;

namespace Vostok.Clusterclient.Core.Helpers
{
    internal class BugcheckException : Exception
    {
        public BugcheckException(string message)
            : base(message)
        {
        }
    }
}