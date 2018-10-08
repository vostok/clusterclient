using System;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Clusterclient.Core.Sending
{
    internal interface IRequestConverter
    {
        [CanBeNull]
        Request TryConvertToAbsolute([NotNull] Request relativeRequest, [NotNull] Uri replica);
    }
}