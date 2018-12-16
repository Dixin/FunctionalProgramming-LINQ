namespace Tutorial.Tests.ParallelLinq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.ParallelLinq;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class PartitioningTests
    {
        [TestMethod]
        public void BuiltInPartitioningTest()
        {
            Partitioning.RangePartitioningForArray();
            Partitioning.ChunkPartitioningForSequence();
            Partitioning.ChunkPartitioningForPartitioner();
            Partitioning.HashPartitioningForGroupBy();
            Partitioning.HashPartitioningForJoin();
            Partitioning.StrippedPartitioningForPartitionerWithSequence();
            Partitioning.StrippedPartitioningForPartitionerWithArray();
        }

        [TestMethod]
        public void StaticPartitionerTest()
        {
            Partitioning.QueryStaticPartitioner();

            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(1, valueCount);
            IEnumerable<int> values = new Partitioning.StaticPartitioner<int>(source)
                .GetPartitions(partitionCount)
                .Select(partition => EnumerableEx.Create(() => partition))
                .Concat()
                .OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }

        [TestMethod]
        public void DynamicPartitionerTest()
        {
            Partitioning.QueryDynamicPartitioner();
            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(1, valueCount);
            IEnumerable<int> partitionsSource = new Partitioning.DynamicPartitioner<int>(source).GetDynamicPartitions();
            IEnumerable<int> values = GetPartitions(partitionsSource, partitionCount).Concat().OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }

        [TestMethod]
        public void OrderablePartitionerTest()
        {
            int partitionCount = Environment.ProcessorCount * 2;
            int valueCount = partitionCount * 10000;
            IEnumerable<int> source = Enumerable.Range(0, valueCount);
            IEnumerable<KeyValuePair<long, int>> partitionsSource = new Partitioning.OrderableDynamicPartitioner<int>(source).GetOrderableDynamicPartitions();
            IEnumerable<KeyValuePair<long, int>> result = GetPartitions(partitionsSource, partitionCount).Concat();
            IOrderedEnumerable<int> indexes = result.Select(value => Convert.ToInt32(value.Key)).OrderBy(index => index);
            EnumerableAssert.AreSequentialEqual(source, indexes);
            IOrderedEnumerable<int> values = result.Select(value => value.Value).OrderBy(value => value);
            EnumerableAssert.AreSequentialEqual(source, values);
        }

        private static IList<IList<TSource>> GetPartitions<TSource>(IEnumerable<TSource> partitionsSource, int partitionCount)
        {
            List<IList<TSource>> partitions = Enumerable
                .Range(0, partitionCount)
                .Select(_ => (IList<TSource>)new List<TSource>())
                .ToList();
            Thread[] partitioningThreads = Enumerable
                .Range(0, partitionCount)
                .Select(_ => partitionsSource.GetEnumerator())
                .Select((partitionIterator, partitionIndex) => new Thread(() =>
                    {
                        IList<TSource> partition = partitions[partitionIndex];
                        using (partitionIterator)
                        {
                            while (partitionIterator.MoveNext())
                            {
                                partition.Add(partitionIterator.Current);
                            }
                        }
                    }))
                .ToArray();
            partitioningThreads.ForEach(thread => thread.Start());
            partitioningThreads.ForEach(thread => thread.Join());
            return partitions;
        }
    }
}
