using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Modules;

namespace Vostok.Clusterclient.Core
{
    public static partial class IClusterClientConfigurationExtensions
    {
        /// <summary>
        /// <para>Adds given <paramref name="module"/> to configuration's <see cref="IClusterClientConfiguration.Modules"/> collection.</para>
        /// <para><paramref name="module"/> will be inserted into request module chain once around the module of specified type.</para>
        /// </summary>
        /// <param name="type">A type of module around which <paramref name="module"/> will be inserted.</param>
        /// <param name="module">A module to insert into request pipeline.</param>
        /// <param name="configuration">A configuration instance.</param>
        /// <param name="position">A relative position of <paramref name="module"/> from module of type <paramref name="type"/>. This parameter is optional and has default value <see cref="ModulePosition.Before"/>.</param>
        public static void AddRequestModule(
            this IClusterClientConfiguration configuration,
            IRequestModule module,
            Type type,
            ModulePosition position = ModulePosition.Before)
        {
            ObtainModules(configuration, type)[position].Add(module);
        }

        /// <summary>
        /// <para>Adds given <paramref name="module"/> to configuration's <see cref="IClusterClientConfiguration.Modules"/> collection.</para>
        /// <para><paramref name="module"/> will be inserted into request module chain once near <paramref name="relatedModule"/>.</para>
        /// </summary>
        /// <param name="relatedModule">A module near which <paramref name="module"/> will be inserted.</param>
        /// <param name="module">A module to insert into request pipeline.</param>
        /// <param name="configuration">A configuration instance.</param>
        /// <param name="position">A relative position of <paramref name="module"/> from <paramref name="relatedModule"/>. This parameter is optional and has default value <see cref="ModulePosition.Before"/>.</param>
        public static void AddRequestModule(
            this IClusterClientConfiguration configuration,
            IRequestModule module,
            RequestModule relatedModule = RequestModule.Logging,
            ModulePosition position = ModulePosition.Before)
        {
            configuration.AddRequestModule(module, RequestModulesMapping.GetModuleType(relatedModule), position);
        }

        private static RelatedModules ObtainModules(IClusterClientConfiguration configuration, Type type)
        {
            if (configuration.Modules == null)
                configuration.Modules = new Dictionary<Type, RelatedModules>();

            if (!configuration.Modules.TryGetValue(type, out var modules))
                configuration.Modules[type] = modules = new RelatedModules();

            return modules;
        }
    }
}