﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Ordering.Weighed
{
    [PublicAPI]
    public class WeighedReplicaOrderingBuilder : IWeighedReplicaOrderingBuilder
    {
        private readonly List<IReplicaWeightModifier> modifiers;

        public WeighedReplicaOrderingBuilder(ILog log)
            : this(null, null, log)
        {
        }

        internal WeighedReplicaOrderingBuilder(string environment, string serviceName, ILog log)
        {
            Log = log;
            MinimumWeight = ClusterClientDefaults.MinimumReplicaWeight;
            MaximumWeight = ClusterClientDefaults.MaximumReplicaWeight;
            InitialWeight = ClusterClientDefaults.InitialReplicaWeight;
            Environment = environment;
            ServiceName = serviceName;

            modifiers = new List<IReplicaWeightModifier>();
        }

        public ILog Log { get; }

        public double MinimumWeight { get; set; }

        public double MaximumWeight { get; set; }

        public double InitialWeight { get; set; }

        public string ServiceName { get; set; }

        public string Environment { get; set; }

        public WeighedReplicaOrdering Build() =>
            new WeighedReplicaOrdering(modifiers, MinimumWeight, MaximumWeight, InitialWeight);

        public void AddModifier(IReplicaWeightModifier modifier)
        {
            if (modifier == null)
                throw new ArgumentNullException(nameof(modifier));

            modifiers.Add(modifier);
        }
    }
}