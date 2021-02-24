using System;
using System.Diagnostics;
using System.Threading;
using JetBrains.Annotations;
using NSubstitute.Exceptions;
using NUnit.Framework;

namespace Vostok.Clusterclient.Core.Tests.Helpers
{
    public static class AssertionExtensions
    {
        public static void ShouldPassIn([NotNull] this Action assertion, TimeSpan wait, TimeSpan pause)
        {
            var watch = Stopwatch.StartNew();

            while (watch.Elapsed < wait)
            {
                try
                {
                    assertion();
                    return;
                }
                catch (AssertionException)
                {
                }
                catch (ReceivedCallsException)
                {
                }

                Thread.Sleep(pause);
            }

            assertion();
        }
    }
}