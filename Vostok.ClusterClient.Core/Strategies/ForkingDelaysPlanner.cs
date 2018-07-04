using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.ClusterClient.Core.Strategies
{
    internal class ForkingDelaysPlanner : IForkingDelaysPlanner
    {
        public static readonly ForkingDelaysPlanner Instance = new ForkingDelaysPlanner();

        public Task Plan(TimeSpan delay, CancellationToken token) => Task.Delay(delay, token);
    }
}