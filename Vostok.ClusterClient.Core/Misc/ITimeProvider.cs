using System;

namespace Vostok.Clusterclient.Core.Misc
{
    internal interface ITimeProvider
    {
        DateTime GetCurrentTime();
    }
}