using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core.Misc
{
    /// <summary>
    /// Describes relative position of <see cref="IRequestModule"/> in request pipeline.
    /// </summary>
    [PublicAPI]
    public enum ModulePosition
    {
        /// <summary>
        /// Indicates that <see cref="IRequestModule"/> should be placed before some other request module.
        /// </summary>
        Before,
        
        /// <summary>
        /// Indicates that <see cref="IRequestModule"/> should be placed after some other request module.
        /// </summary>
        After
    }
}