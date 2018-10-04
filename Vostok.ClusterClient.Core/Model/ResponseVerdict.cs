using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Model
{
    /// <summary>
    /// Represents a decision about response quality.
    /// </summary>
    [PublicAPI]
    public enum ResponseVerdict
    {
        /// <summary>
        /// Provided response can be safely returned to client as operation's final result.
        /// </summary>
        Accept = 0,

        /// <summary>
        /// Provided response shouldn't be returned to client. Another replica must be contacted if possible.
        /// </summary>
        Reject = 1,

        /// <summary>
        /// Inconclusive result. Decision is up to next criterion.
        /// </summary>
        DontKnow = 2
    }
}