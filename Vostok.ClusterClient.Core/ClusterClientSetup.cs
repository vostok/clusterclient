namespace Vostok.Clusterclient.Core
{
    /// <summary>
    /// Delegate which configure <see cref="IClusterClientConfiguration"/>.
    /// </summary>
    /// <param name="configuration"></param>
    public delegate void ClusterClientSetup(IClusterClientConfiguration configuration);
}