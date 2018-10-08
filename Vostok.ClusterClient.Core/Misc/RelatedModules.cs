using System.Collections.Generic;
using Vostok.ClusterClient.Core.Modules;

namespace Vostok.ClusterClient.Core.Misc
{
    /// <summary>
    /// Contains <see cref="IRequestModule"/>'s that will be inserted into request module chain near some other module.
    /// </summary>
    public class RelatedModules
    {
        /// <summary>
        /// A <see cref="IRequestModule"/>'s that will be inserted into request module chain before some other module.
        /// </summary>
        public List<IRequestModule> Before { get; } = new List<IRequestModule>();
        
        /// <summary>
        /// A <see cref="IRequestModule"/>'s that will be inserted into request module chain after some other module.
        /// </summary>
        public List<IRequestModule> After { get; } = new List<IRequestModule>();
    }
}