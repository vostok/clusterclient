using JetBrains.Annotations;

namespace Vostok.ClusterClient.Core.Modules
{
    /// <summary>
    /// <para>Defines request pipeline extension point.</para>
    /// </summary>
    [PublicAPI]
    public enum RequestPipelinePoint
    {
        /// <summary>
        /// A point in request pipeline after request transformation.
        /// </summary>
        AfterPrepareRequest = 0,
        /// <summary>
        /// A point in request pipeline after <see cref="RequestValidationModule"/>.
        /// </summary>
        AfterRequestValidation = 1,
        /// <summary>
        /// A point in request pipeline before <see cref="AbsoluteUrlSenderModule"/>.
        /// </summary>
        BeforeSend = 2,
        /// <summary>
        /// A point in request pipeline before <see cref="RequestExecutionModule"/>.
        /// </summary>
        BeforeExecution = 3,
    }
}