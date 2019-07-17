using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Misc;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Storage;
using Vostok.Commons.Collections;
using Vostok.Commons.Threading;

namespace Vostok.Clusterclient.Core.Ordering.Weighed
{
    /// <summary>
    /// <para>Represents a probabilistic ordering which orders replicas based on their weights.</para>
    /// <para>A weight is a number in customizable <c>[min-weight; max-weight]</c> range where <c>min-weight >= 0</c>. </para>
    /// <para>Weight for each replica is calculated by taking a constant initial value and modifying it with a chain of <see cref="IReplicaWeightModifier"/>s. Range limits are enforced after applying each modifier.</para>
    /// <para>Weights are used to compute replica selection probabilities. A replica with weight = <c>1.0</c> is twice more likely to come first than a replica with weight = <c>0.5</c>.</para>
    /// <para>If all replicas have same weight, the ordering essentially becomes random.</para>
    /// <para>Replicas with weight = <see cref="double.PositiveInfinity"/> come first regardless of other replica weights.</para>
    /// <para>Replicas with weight = <c>0</c> come last regardless of other replica weights.</para>
    /// <para>Developer's goal is to manipulate replica weights via <see cref="IReplicaWeightModifier"/>s to produce best possible results.</para>
    /// <para>Feedback from <see cref="Learn"/> method is forwarded to all modifiers.</para>
    /// </summary>
    [PublicAPI]
    public class WeighedReplicaOrdering : IReplicaOrdering
    {
        private const int PooledArraySize = 50;

        private static readonly UnboundedObjectPool<TreeNode[]> TreeArrays = new UnboundedObjectPool<TreeNode[]>(() => new TreeNode[PooledArraySize]);

        private readonly IList<IReplicaWeightModifier> modifiers;
        private readonly IReplicaWeightCalculator weightCalculator;

        /// <param name="modifiers">A chain of <see cref="IReplicaWeightModifier"/> which will be used to modify replica weight.</param>
        /// <param name="minimumWeight">A minimal possible weight of replica. This parameter is optional and has default value <see cref="ClusterClientDefaults.MinimumReplicaWeight"/>.</param>
        /// <param name="maximumWeight">A maximal possible weight of replica. This parameter is optional and has default value <see cref="ClusterClientDefaults.MaximumReplicaWeight"/>.</param>
        /// <param name="initialWeight">A initial weight of replica. This parameter is optional and has default value <see cref="ClusterClientDefaults.InitialReplicaWeight"/>.</param>
        public WeighedReplicaOrdering(
            [NotNull] IList<IReplicaWeightModifier> modifiers,
            double minimumWeight = ClusterClientDefaults.MinimumReplicaWeight,
            double maximumWeight = ClusterClientDefaults.MaximumReplicaWeight,
            double initialWeight = ClusterClientDefaults.InitialReplicaWeight)
            : this(modifiers, new ReplicaWeightCalculator(modifiers, minimumWeight, maximumWeight, initialWeight))
        {
        }

        internal WeighedReplicaOrdering(
            [NotNull] IList<IReplicaWeightModifier> modifiers,
            [NotNull] IReplicaWeightCalculator weightCalculator)
        {
            this.modifiers = modifiers;
            this.weightCalculator = weightCalculator;
        }

        /// <inheritdoc />
        public void Learn(ReplicaResult result, IReplicaStorageProvider storageProvider)
        {
            foreach (var modifier in modifiers)
                modifier.Learn(result, storageProvider);
        }

        /// <inheritdoc />
        public IEnumerable<Uri> Order(IList<Uri> replicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters)
        {
            if (replicas.Count < 2)
                return replicas;

            var requiredCapacity = replicas.Count * 2 - 1;
            if (requiredCapacity > PooledArraySize)
                return OrderInternal(replicas, storageProvider, request, parameters, new TreeNode[requiredCapacity]);

            return OrderUsingPooledArray(replicas, storageProvider, request, parameters);
        }

        private static void CleanupTree(TreeNode[] tree)
        {
            for (var i = 0; i < tree.Length; i++)
            {
                tree[i].Exists = false;
                tree[i].Replica = null;
                tree[i].Weight = 0;
            }
        }

        private static Uri SelectReplicaFromTree(TreeNode[] tree)
        {
            var weightsSum = tree[0].Weight;
            var randomPoint = ThreadSafeRandom.NextDouble() * weightsSum;
            var index = 0;
            var leftBehind = 0.0;

            while (true)
            {
                var node = tree[index];
                if (!node.Exists)
                    throw new BugcheckException("Attempt to select a replica from empty tree. Surely, this is a bug in code.");

                if (node.IsLeafNode)
                {
                    RemoveLeafFromTree(tree, index);
                    return node.Replica;
                }

                var leftChildNode = GetLeftChildNodeIfExists(tree, index);
                if (leftChildNode.HasValue && leftChildNode.Value.Weight >= randomPoint - leftBehind)
                {
                    index = GetLeftChildIndex(index);
                    continue;
                }

                var rightChildNode = GetRightChildNodeIfExists(tree, index);
                if (rightChildNode.HasValue)
                {
                    if (leftChildNode.HasValue)
                        leftBehind += leftChildNode.Value.Weight;

                    index = GetRightChildIndex(index);
                    continue;
                }

                throw new BugcheckException("A non-leaf tree node does not have any children. Surely, this is a bug in code.");
            }
        }

        private static void RemoveLeafFromTree(TreeNode[] tree, int index)
        {
            var leafWeight = tree[index].Weight;

            tree[index].Exists = false;

            // (iloktionov): После удаления листа необходимо сделать две вещи:
            // (iloktionov): 1. Вычесть его вес по цепочке вверх вплоть до корня дерева.
            // (iloktionov): 2. Удалить все промежуточные ноды, которые теперь не связаны с листьями.
            while (index > 0)
            {
                index = GetParentIndex(index);

                tree[index].Weight -= leafWeight;

                if (GetLeftChildNodeIfExists(tree, index).HasValue)
                    continue;

                if (GetRightChildNodeIfExists(tree, index).HasValue)
                    continue;

                tree[index].Exists = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TreeNode? GetLeftChildNodeIfExists(TreeNode[] tree, int parentIndex)
        {
            var leftChildIndex = GetLeftChildIndex(parentIndex);
            if (leftChildIndex >= tree.Length)
                return null;

            var leftChildNode = tree[leftChildIndex];
            if (leftChildNode.Exists)
                return leftChildNode;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TreeNode? GetRightChildNodeIfExists(TreeNode[] tree, int parentIndex)
        {
            var rightChildIndex = GetRightChildIndex(parentIndex);
            if (rightChildIndex >= tree.Length)
                return null;

            var rightChildNode = tree[rightChildIndex];
            if (rightChildNode.Exists)
                return rightChildNode;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetParentIndex(int childIndex) => (childIndex - 1) / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLeftChildIndex(int parentIndex) => parentIndex * 2 + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetRightChildIndex(int parentIndex) => parentIndex * 2 + 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Shuffle(List<Uri> replicas)
        {
            for (var i = 0; i < replicas.Count - 1; i++)
                Swap(replicas, i, ThreadSafeRandom.Next(i, replicas.Count));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(List<Uri> replicas, int i, int j)
        {
            var temp = replicas[i];
            replicas[i] = replicas[j];
            replicas[j] = temp;
        }

        private IEnumerable<Uri> OrderUsingPooledArray(IList<Uri> replicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters)
        {
            using (TreeArrays.Acquire(out var treeArray))
                foreach (var replica in OrderInternal(replicas, storageProvider, request, parameters, treeArray))
                    yield return replica;
        }

        private IEnumerable<Uri> OrderInternal(IList<Uri> replicas, IReplicaStorageProvider storageProvider, Request request, RequestParameters parameters, TreeNode[] tree)
        {
            var replicasWithInfiniteWeight = null as List<Uri>;
            var replicasWithZeroWeight = null as List<Uri>;

            CleanupTree(tree);

            // (iloktionov): Построим суммирующее дерево отрезков, листьями в котором будут являться реплики со своими весами. В корне этого дерева будет сумма весов всех реплик. 
            BuildTree(tree, replicas, storageProvider, request, parameters, ref replicasWithInfiniteWeight, ref replicasWithZeroWeight);

            // (iloktionov): Реплики с бесконечным весом должны иметь безусловный приоритет, при этом случайно переупорядочиваясь между собой:
            if (replicasWithInfiniteWeight != null)
            {
                Shuffle(replicasWithInfiniteWeight);

                foreach (var replica in replicasWithInfiniteWeight)
                    yield return replica;
            }

            var replicasToSelectFromTree = replicas.Count;

            replicasToSelectFromTree -= replicasWithInfiniteWeight?.Count ?? 0;
            replicasToSelectFromTree -= replicasWithZeroWeight?.Count ?? 0;

            for (var i = 0; i < replicasToSelectFromTree; i++)
                yield return SelectReplicaFromTree(tree);

            // (iloktionov): Реплики с нулевым весом должны идти последними, при этом случайно переупорядочиваясь между собой:
            if (replicasWithZeroWeight != null)
            {
                Shuffle(replicasWithZeroWeight);

                foreach (var replica in replicasWithZeroWeight)
                    yield return replica;
            }
        }

        private void BuildTree(
            TreeNode[] tree,
            IList<Uri> replicas,
            IReplicaStorageProvider storageProvider,
            Request request,
            RequestParameters parameters,
            ref List<Uri> replicasWithInfiniteWeight,
            ref List<Uri> replicasWithZeroWeight)
        {
            var firstLeafIndex = FillLeaves(tree, replicas, storageProvider, request, parameters, 
                ref replicasWithInfiniteWeight, ref replicasWithZeroWeight);

            GetWeight(tree, 0, firstLeafIndex);
        }

        /// <summary>
        /// Returns first leaf index.
        /// </summary>
        private int FillLeaves(
            TreeNode[] tree,
            IList<Uri> replicas,
            IReplicaStorageProvider storageProvider,
            Request request,
            RequestParameters parameters,
            ref List<Uri> replicasWithInfiniteWeight,
            ref List<Uri> replicasWithZeroWeight)
        {
            var firstLeafIndex = replicas.Count - 1;
            var currentLeafIndex = firstLeafIndex;

            foreach (var replica in replicas)
            {
                var weight = weightCalculator.GetWeight(replica, replicas, storageProvider, request, parameters);
                if (weight < 0.0)
                    throw new BugcheckException($"A negative weight has been calculated for replica '{replica}': {weight}.");

                // (iloktionov): Бесконечности портят расчёты на дереве, поэтому они обрабатываются отдельно и не вставляются в него:
                if (double.IsPositiveInfinity(weight))
                {
                    (replicasWithInfiniteWeight ?? (replicasWithInfiniteWeight = new List<Uri>())).Add(replica);
                    continue;
                }

                // (iloktionov): Чтобы избежать детерминированного упорядочивания реплик с нулевым весом, их тоже придётся рассмотреть отдельно:
                if (weight < double.Epsilon)
                {
                    (replicasWithZeroWeight ?? (replicasWithZeroWeight = new List<Uri>())).Add(replica);
                    continue;
                }

                // (iloktionov): Заполняем листовую ноду дерева:
                tree[currentLeafIndex++] = new TreeNode
                {
                    Exists = true,
                    Weight = weight,
                    Replica = replica
                };
            }

            return firstLeafIndex;
        }

        private static double GetWeight(TreeNode[] tree, int index, int firstLeafIndex)
        {
            if (index >= firstLeafIndex)
                return tree[index].Exists ? tree[index].Weight : 0;

            var weight =
                GetWeight(tree, GetLeftChildIndex(index), firstLeafIndex) +
                GetWeight(tree, GetRightChildIndex(index), firstLeafIndex);

            tree[index].Exists = true;
            tree[index].Weight = weight;

            return weight;
        }

        private struct TreeNode
        {
            public bool Exists;
            public double Weight;
            public Uri Replica;
            public bool IsLeafNode => Replica != null;
        }
    }
}