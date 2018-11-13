using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Modules
{
    /// <summary>
    /// <para>Represents a module which can be injected into request execution pipeline.</para>
    /// <para>All modules are wired in a chain where each one has a delegate to call the next module.</para>
    /// </summary>
    [PublicAPI]
    public interface IRequestModule
    {
        /// <summary>
        /// <para>Executes the module using given <paramref name="context"/> and a delegate to call <paramref name="next"/> module.</para>
        /// <para>The module may either await and return the result of next one or return a custom result without calling next module.</para>
        /// <para>Implementations of this method MUST BE thread-safe.</para>
        /// </summary>
        [ItemNotNull]
        Task<ClusterResult> ExecuteAsync([NotNull] IRequestContext context, [NotNull] Func<IRequestContext, Task<ClusterResult>> next);
    }
}