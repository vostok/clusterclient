using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Vostok.Clusterclient.Core.Ordering.Weighed.Relative
{
    internal static class InterlockedEx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Read(ref double location)
            => Interlocked.CompareExchange(ref location, 0d, 0d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(ref double location, double value)
        {
            var newCurrentValue = location;

            while (true)
            {
                var currentValue = newCurrentValue;
                var newValue = currentValue + value;

                newCurrentValue = Interlocked.CompareExchange(ref location, newValue, currentValue);

                if (Math.Abs(newCurrentValue - currentValue) < double.Epsilon)
                    return;
            }
        }
    }
}