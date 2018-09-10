using System;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal class OperationCanceledByServerException : OperationCanceledException
    {
        public OperationCanceledByServerException() : base("Canceled by server")
        {
        }
    }
}