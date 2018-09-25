using System;
using Vostok.Logging.Abstractions;

namespace Vostok.ClusterClient.Core.Ordering.Weighed.Adaptive
{
    /// <summary>
    /// <para>An implementation of adaptive health which uses numbers in <c>(0; 1]</c> range as health values. Default health value is equal to 1.</para>
    /// <para>Upon increase, health value is multiplied by an up multilplier in <c>(1; +infinity)</c> range.</para>
    /// <para>Upon decrease, health value is multiplied by a down multilplier in <c>(0; 1)</c> range.</para>
    /// <para>Health values have a customizable lower bound in <c>(0; 1)</c> range. This ensures that bad replicas have a chance to improve their health.</para>
    /// <para>Health application is just a multiplication of health value and current weight (health = 0.5 causes weight = 2 to turn into 1).</para>
    /// <para>This health implementation can only decrease replica weights as it's aim is to avoid misbehaving replicas.</para>
    /// </summary>
    public class AdaptiveHealthWithoutDecay : IAdaptiveHealthImplementation<double>
    {
        private const double MaximumHealthValue = 1.0;

        /// <param name="upMultiplier">A multiplier used to increase health. Must be in <c>(1; +infinity)</c> range.</param>
        /// <param name="downMultiplier">A multiplier used to decrease health. Must be in <c>(0; 1)</c> range.</param>
        /// <param name="minimumHealthValue">Minimum possible health value. Must be in <c>(0; 1)</c> range.</param>
        public AdaptiveHealthWithoutDecay(
            double upMultiplier = ClusterClientDefaults.AdaptiveHealthUpMultiplier,
            double downMultiplier = ClusterClientDefaults.AdaptiveHealthDownMultiplier,
            double minimumHealthValue = ClusterClientDefaults.AdaptiveHealthMinimumValue)
        {
            if (upMultiplier <= 1.0)
                throw new ArgumentOutOfRangeException(nameof(upMultiplier), $"Up multiplier must be > 1. Given value = '{upMultiplier}'.");

            if (downMultiplier <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(downMultiplier), $"Down multiplier must be positive. Given value = '{downMultiplier}'.");

            if (downMultiplier >= 1.0)
                throw new ArgumentOutOfRangeException(nameof(downMultiplier), $"Down multiplier must be < 1. Given value = '{downMultiplier}'.");

            if (minimumHealthValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumHealthValue), $"Minimum health must be positive. Given value = '{minimumHealthValue}'.");

            if (minimumHealthValue >= 1)
                throw new ArgumentOutOfRangeException(nameof(minimumHealthValue), $"Minimum health must be < 1. Given value = '{minimumHealthValue}'.");

            UpMultiplier = upMultiplier;
            DownMultiplier = downMultiplier;
            MinimumHealthValue = minimumHealthValue;
        }

        /// <summary>
        /// A multiplier used to increase health. Must be in <c>(1; +infinity)</c> range.
        /// </summary>
        public double UpMultiplier { get; }

        /// <summary>
        /// A multiplier used to decrease health. Must be in <c>(0; 1)</c> range.
        /// </summary>
        public double DownMultiplier { get; }

        /// <summary>
        /// Minimum possible health value. Must be in <c>(0; 1)</c> range.
        /// </summary>
        public double MinimumHealthValue { get; }

        /// <inheritdoc />
        public void ModifyWeight(double health, ref double weight) => weight *= health;

        /// <inheritdoc />
        public double CreateDefaultHealth() => MaximumHealthValue;

        /// <inheritdoc />
        public double IncreaseHealth(double current) =>
            Math.Min(MaximumHealthValue, current * UpMultiplier);

        /// <inheritdoc />
        public double DecreaseHealth(double current) =>
            Math.Max(MinimumHealthValue, current * DownMultiplier);

        /// <inheritdoc />
        public bool AreEqual(double x, double y) => x.Equals(y);

        /// <inheritdoc />
        public void LogHealthChange(Uri replica, double oldHealth, double newHealth, ILog log) =>
            log.Debug($"Local health for replica '{replica}' has changed from {oldHealth:N4} to {newHealth:N4}.");
    }
}