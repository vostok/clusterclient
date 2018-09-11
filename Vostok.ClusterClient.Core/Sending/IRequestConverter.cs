using System;
using JetBrains.Annotations;
using Vostok.ClusterClient.Core.Model;

namespace Vostok.ClusterClient.Core.Sending
{
    internal interface IRequestConverter
    {
        [CanBeNull]
        Request TryConvertToAbsolute([NotNull] Request relativeRequest, [NotNull] Uri replica);
    }
}