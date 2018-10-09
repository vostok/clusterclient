using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Misc
{
    /// <summary>
    /// Contains <see cref="IRequestModule"/>'s that will be inserted into request module chain near some other module.
    /// </summary>
    [PublicAPI]
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

        internal List<IRequestModule> this[ModulePosition position]
        {
            get
            {
                switch (position)
                {
                    case ModulePosition.Before:
                        return Before;
                    case ModulePosition.After:
                        return After;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(position), position, null);
                }
            }
        }

        internal int Count => Before.Count + After.Count;
    }
}