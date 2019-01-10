namespace Tutorial.LinqToObjects
{
    using System;

    internal static partial class IteratorPattern
    {
        internal abstract class Sequence
        {
            public abstract Iterator GetEnumerator(); // Must be public.
        }

        internal abstract class Iterator
        {
            public abstract bool MoveNext(); // Must be public.

            public abstract object Current { get; } // Must be public.
        }

        internal abstract class GenericSequence<T>
        {
            public abstract GenericIterator<T> GetEnumerator(); // Must be public.
        }

        internal abstract class GenericIterator<T>
        {
            public abstract bool MoveNext(); // Must be public.

            public abstract T Current { get; } // Must be public.
        }

        internal static void ForEach<T>(Sequence sequence, Action<T> process)
        {
            foreach (T value in sequence)
            {
                process(value);
            }
        }

        internal static void ForEach<T>(GenericSequence<T> sequence, Action<T> process)
        {
            foreach (T value in sequence)
            {
                process(value);
            }
        }

        internal static void CompiledForEach<T>(Sequence sequence, Action<T> process)
        {
            Iterator iterator = sequence.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                {
                    T value = (T)iterator.Current;
                    process(value);
                }
            }
            finally
            {
                (iterator as IDisposable)?.Dispose();
            }
        }

        internal static void CompiledForEach<T>(GenericSequence<T> sequence, Action<T> process)
        {
            GenericIterator<T> iterator = sequence.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                {
                    T value = iterator.Current;
                    process(value);
                }
            }
            finally
            {
                (iterator as IDisposable)?.Dispose();
            }
        }

        internal class LinkedListNode<T>
        {
            internal LinkedListNode(T value, LinkedListNode<T> next = null) =>
                (this.Value, this.Next) = (value, next);

            public T Value { get; }

            public LinkedListNode<T> Next { get; }
        }

        internal class LinkedListSequence<T> : GenericSequence<T>
        {
            private readonly LinkedListNode<T> head;

            internal LinkedListSequence(LinkedListNode<T> head) => this.head = head;

            public override GenericIterator<T> GetEnumerator() => new LinkedListIterator<T>(this.head);
        }

        internal class LinkedListIterator<T> : GenericIterator<T>
        {
            private LinkedListNode<T> node; // State.

            internal LinkedListIterator(LinkedListNode<T> head) =>
                this.node = new LinkedListNode<T>(default, head);

            public override bool MoveNext()
            {
                if (this.node.Next != null)
                {
                    this.node = this.node.Next; // State change.
                    return true;
                }
                return false;
            }

            public override T Current => this.node.Value;
        }

        internal static void ForEach()
        {
            LinkedListNode<int> node3 = new LinkedListNode<int>(3, null);
            LinkedListNode<int> node2 = new LinkedListNode<int>(2, node3);
            LinkedListNode<int> node1 = new LinkedListNode<int>(1, node2);
            LinkedListSequence<int> sequence = new LinkedListSequence<int>(node1);
            foreach (int value in sequence)
            {
                value.WriteLine(); // 1 2 3
            }
        }

        internal static void ForEach<T>(T[] array, Action<T> process)
        {
            foreach (T value in array)
            {
                process(value);
            }
        }

        internal static void CompiledForEach<T>(T[] array, Action<T> process)
        {
            for (int index = 0; index < array.Length; index++)
            {
                T value = array[index];
                process(value);
            }
        }

        internal static void ForEach(string @string, Action<char> process)
        {
            foreach (char value in @string)
            {
                process(value);
            }
        }

        internal static void CompiledForEach(string @string, Action<char> process)
        {
            for (int index = 0; index < @string.Length; index++)
            {
                char value = @string[index];
                process(value);
            }
        }
    }
}

#if DEMO
namespace System.Collections
{
    public interface IEnumerable // Sequence.
    {
        IEnumerator GetEnumerator();
    }

    public interface IEnumerator // Iterator.
    {
        object Current { get; }

        bool MoveNext();

        void Reset(); // For COM interoperability.
    }
}

namespace System
{
    public interface IDisposable
    {
        void Dispose();
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerable<T> : IEnumerable // Sequence.
    {
        IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<T> : IDisposable, IEnumerator // Iterator.
    {
        T Current { get; }
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerable<out T> : IEnumerable // Sequence.
    {
        IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<out T> : IDisposable, IEnumerator // Iterator.
    {
        T Current { get; }
    }
}
#endif