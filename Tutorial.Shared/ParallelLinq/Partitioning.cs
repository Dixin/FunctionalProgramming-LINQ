namespace Tutorial.ParallelLinq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using static Visualizer;

    using Parallel = System.Threading.Tasks.Parallel;

    internal static partial class Partitioning
    {
        internal static void Range()
        {
            int[] array = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray();
            array.AsParallel()
                .Visualize(ParallelEnumerable.Select, value => ComputingWorkload(value))
                .WriteLines();
        }

        internal static void Strip()
        {
            IEnumerable<int> sequence = Enumerable.Range(0, Environment.ProcessorCount * 4);
            sequence.AsParallel()
                .Visualize(ParallelEnumerable.Select, value => ComputingWorkload(value))
                .WriteLines();
        }

        internal static void StripForArray()
        {
            int[] array = Enumerable.Range(0, Environment.ProcessorCount * 4).ToArray();
            Partitioner.Create(array, loadBalance: true).AsParallel()
                .Visualize(ParallelEnumerable.Select, value => ComputingWorkload(value))
                .WriteLines();
        }

        internal readonly struct Data
        {
            internal Data(int value) => this.Value = value;

            internal int Value { get; }

            public override int GetHashCode() => this.Value % Environment.ProcessorCount;

            public override string ToString() => this.Value.ToString(); // For span label.
        }

        internal static void HashForGroupBy()
        {
            IEnumerable<Data> sequence = new int[] { 0, 1, 2, 2, 2, 2, 3, 4, 5, 6, 10 }
                .Select(value => new Data(value));
            sequence.AsParallel()
                .Visualize(
                    (source, elementSelector) => source.GroupBy(
                        keySelector: data => data, // Key's GetHashCode is called.
                        elementSelector: elementSelector),
                    data => ComputingWorkload(data.Value).ToString()) // elementSelector.
                .WriteLines(group => string.Join(", ", group));
            // Equivalent to:
            // MarkerSeries markerSeries = Markers.CreateMarkerSeries("Parallel");
            // source.AsParallel()
            //    .GroupBy(
            //        keySelector: data => data,
            //        elementSelector: data =>
            //        {
            //            using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, data.ToString()))
            //            {
            //                return ComputingWorkload(data.Value);
            //            }
            //        })
            //    .WriteLines(group => string.Join(", ", group));
        }

        internal static void HashInJoin()
        {
            IEnumerable<Data> outerSequence = new int[] { 0, 1, 2, 2, 2, 2, 3, 6 }.Select(value => new Data(value));
            IEnumerable<Data> innerSequence = new int[] { 4, 5, 6, 7 }.Select(value => new Data(value));
            outerSequence.AsParallel()
                .Visualize(
                    (source, resultSelector) => source
                        .Join(
                            inner: innerSequence.AsParallel(),
                            outerKeySelector: data => data, // Key's GetHashCode is called.
                            innerKeySelector: data => data, // Key's GetHashCode is called.
                            resultSelector: (outerData, innerData) => resultSelector(outerData)),
                    data => ComputingWorkload(data.Value)) // resultSelector.
                .WriteLines();
        }

        internal static void Chunk()
        {
            IEnumerable<int> source = Enumerable.Range(0, (1 + 2) * 3 * Environment.ProcessorCount + 3);
            Partitioner.Create(source, EnumerablePartitionerOptions.None).AsParallel()
                .Visualize(ParallelEnumerable.Select, value => ComputingWorkload())
                .WriteLines();
        }

        internal class StaticPartitioner<TSource> : Partitioner<TSource>
        {
            protected readonly IBuffer<TSource> buffer;

            internal StaticPartitioner(IEnumerable<TSource> source) => this.buffer = source.Share();

            public override IList<IEnumerator<TSource>> GetPartitions(int partitionCount)
            {
                if (partitionCount <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(partitionCount));
                }

                return Enumerable
                    .Range(0, partitionCount)
                    .Select(_ => this.buffer.GetEnumerator())
                    .ToArray();
            }
        }

        internal class DynamicPartitioner<TSource> : StaticPartitioner<TSource>
        {
            internal DynamicPartitioner(IEnumerable<TSource> source) : base(source) { }

            public override bool SupportsDynamicPartitions => true;

            public override IEnumerable<TSource> GetDynamicPartitions() => this.buffer;
        }

        internal static void QueryStaticPartitioner()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            new StaticPartitioner<int>(source).AsParallel()
                .Visualize(ParallelEnumerable.Select, value => ComputingWorkload(value))
                .WriteLines();
        }

        internal static void QueryDynamicPartitioner()
        {
            IEnumerable<int> source = Enumerable.Range(0, Environment.ProcessorCount * 4);
            Parallel.ForEach(new DynamicPartitioner<int>(source), value => ComputingWorkload(value));
        }

        internal class OrderableDynamicPartitioner<TSource> : OrderablePartitioner<TSource>
        {
            private readonly IBuffer<KeyValuePair<long, TSource>> buffer;

            internal OrderableDynamicPartitioner(IEnumerable<TSource> source)
                : base(keysOrderedInEachPartition: true, keysOrderedAcrossPartitions: true, keysNormalized: true)
            {
                long index = -1;
                this.buffer = source
                    .Select(value => new KeyValuePair<long, TSource>(Interlocked.Increment(ref index), value))
                    .Share();
            }

            public override bool SupportsDynamicPartitions => true;

            public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(
                int partitionCount) => Enumerable
                .Range(0, partitionCount)
                .Select(_ => this.buffer.GetEnumerator())
                .ToArray();

            public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions() => this.buffer;
        }

        internal static void QueryOrderablePartitioner()
        {
            int[] source = Enumerable.Range(0, Environment.ProcessorCount * 2).ToArray();
            new OrderableDynamicPartitioner<int>(source)
                .AsParallel()
                .Select(value => value + ComputingWorkload())
                .WriteLines(); // 1 0 5 3 4 6 2 7

            new OrderableDynamicPartitioner<int>(source)
                .AsParallel()
                .AsOrdered()
                .Select(value => value + ComputingWorkload())
                .WriteLines(); // 0 ... 7

            new DynamicPartitioner<int>(source)
                .AsParallel()
                .AsOrdered()
                .Select(value => value + ComputingWorkload())
                .WriteLines();
            // InvalidOperationException: AsOrdered may not be used with a partitioner that is not orderable.
        }
    }
}

#if DEMO
namespace System.Collections.Concurrent
{
    using System.Collections.Generic;

    public abstract class Partitioner<TSource>
    {
        protected Partitioner() { }

        public virtual bool SupportsDynamicPartitions => false;

        public abstract IList<IEnumerator<TSource>> GetPartitions(int partitionCount);

        public virtual IEnumerable<TSource> GetDynamicPartitions() =>
            throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");
    }
}

namespace System.Threading.Tasks
{
    using System.Collections.Concurrent;

    public static class Parallel
    {
        public static ParallelLoopResult ForEach<TSource>(Partitioner<TSource> source, Action<TSource> body);
    }
}

namespace System.Collections.Concurrent
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class OrderablePartitioner<TSource> : Partitioner<TSource>
    {
        protected OrderablePartitioner(bool keysOrderedInEachPartition, bool keysOrderedAcrossPartitions, bool keysNormalized)
        {
            this.KeysOrderedInEachPartition = keysOrderedInEachPartition;
            this.KeysOrderedAcrossPartitions = keysOrderedAcrossPartitions;
            this.KeysNormalized = keysNormalized;
        }

        public bool KeysNormalized { get; }

        public bool KeysOrderedInEachPartition { get; }

        public bool KeysOrderedAcrossPartitions { get; }

        public abstract IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount);

        public virtual IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions() =>
            throw new NotSupportedException("Dynamic partitions are not supported by this partitioner.");

        public override IList<IEnumerator<TSource>> GetPartitions(
            int partitionCount) => this.GetOrderablePartitions(partitionCount)
                .Select(partition => new EnumeratorDropIndices(partition))
                .ToArray();


        public override IEnumerable<TSource> GetDynamicPartitions() => 
            new EnumerableDropIndices(this.GetOrderableDynamicPartitions());

        private class EnumerableDropIndices : IEnumerable<TSource>
        {
            private readonly IEnumerable<KeyValuePair<long, TSource>> source;

            public EnumerableDropIndices(IEnumerable<KeyValuePair<long, TSource>> source)
            {
                this.source = source;
            }

            public IEnumerator<TSource> GetEnumerator() => new EnumeratorDropIndices(this.source.GetEnumerator());

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        private class EnumeratorDropIndices : IEnumerator<TSource>
        {
            private readonly IEnumerator<KeyValuePair<long, TSource>> source;

            public TSource Current => this.source.Current.Value;

            object IEnumerator.Current => this.Current;

            public EnumeratorDropIndices(IEnumerator<KeyValuePair<long, TSource>> source)
            {
                this.source = source;
            }

            public bool MoveNext() => this.source.MoveNext();

            public void Dispose() => this.source.Dispose();

            public void Reset() => this.source.Reset();
        }
    }
}
#endif