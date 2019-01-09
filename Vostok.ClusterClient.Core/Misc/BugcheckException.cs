using System;

namespace Vostok.Clusterclient.Core.Misc
{
    internal class BugcheckException : Exception
    {
        public BugcheckException(string message)
            : base(message)
        {
        }
    }
}