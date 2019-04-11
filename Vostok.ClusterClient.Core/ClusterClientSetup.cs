namespace Vostok.Clusterclient.Core
{
    /// <summary>
    /// Delegate which configures <see cref="IClusterClientConfiguration"/>.
    /// </summary>
    public delegate void ClusterClientSetup(IClusterClientConfiguration configuration);
}