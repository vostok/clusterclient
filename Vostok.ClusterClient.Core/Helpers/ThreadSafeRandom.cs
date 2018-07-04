using System;

namespace Vostok.ClusterClient.Core.Helpers
{
    internal static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random random;

        public static double NextDouble() =>
            ObtainRandom().NextDouble();

        public static int Next() =>
            ObtainRandom().Next();

        public static int Next(int maxValue) =>
            ObtainRandom().Next(maxValue);

        public static int Next(int minValue, int maxValue) =>
            ObtainRandom().Next(minValue, maxValue);

        private static Random ObtainRandom() =>
            random ?? (random = new Random(Guid.NewGuid().GetHashCode()));
    }
}