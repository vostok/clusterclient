﻿using System;
using System.Collections.Generic;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Topology.TargetEnvironment;
using Vostok.Logging.Abstractions;

namespace Vostok.Clusterclient.Core.Ordering.Weighed
{
    internal class WeighedReplicaOrderingBuilder : IWeighedReplicaOrderingBuilder
    {
        private readonly List<IReplicaWeightModifier> modifiers;

        public WeighedReplicaOrderingBuilder(ILog log)
            : this(null, null, log)
        {
        }

        public WeighedReplicaOrderingBuilder(ITargetEnvironmentProvider environmentProvider, string serviceName, ILog log)
        {
            Log = log;
            MinimumWeight = ClusterClientDefaults.MinimumReplicaWeight;
            MaximumWeight = ClusterClientDefaults.MaximumReplicaWeight;
            InitialWeight = ClusterClientDefaults.InitialReplicaWeight;
            EnvironmentProvider = environmentProvider;
            ServiceName = serviceName;

            modifiers = new List<IReplicaWeightModifier>();
        }

        public ILog Log { get; }

        public double MinimumWeight { get; set; }

        public double MaximumWeight { get; set; }

        public double InitialWeight { get; set; }

        public string ServiceName { get; set; }

        public string Environment { get; set; }

        public ITargetEnvironmentProvider EnvironmentProvider { get; set; }

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