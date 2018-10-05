using System.Collections.Generic;
using Vostok.ClusterClient.Core.Modules;

namespace Vostok.ClusterClient.Core.Misc
{
    public class RelatedModules
    {
        public List<IRequestModule> Before { get; } = new List<IRequestModule>();
        public List<IRequestModule> After { get; } = new List<IRequestModule>();
    }
}