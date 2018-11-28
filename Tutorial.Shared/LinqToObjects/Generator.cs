namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public enum IteratorState
    {
        Create = -2,
        Start = 0,
        MoveNext = 1,
        End = -1,
        Error = -3
    }

    public class Iterator<T> : IEnumerator<T>
    {
        protected readonly Action start;

        protected readonly Func<bool> moveNext;

        protected readonly Func<T> getCurrent;

        protected readonly Action dispose;

        protected readonly Action end;

        public Iterator(
            Action start = null,
            Func<bool> moveNext = null,
            Func<T> getCurrent = null,
            Action dispose = null,
            Action end = null)
        {
            this.start = start;
            this.moveNext = moveNext;
            this.getCurrent = getCurrent;
            this.dispose = dispose;
            this.end = end;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => this.Current;

        internal IteratorState State { get; private set; } = IteratorState.Create; // IteratorState: Create.

        internal Iterator<T> Start()
        {
            this.State = IteratorState.Start;  // IteratorState: Create => Start.
            return this;
        }

        public bool MoveNext()
        {
            try
            {
                switch (this.State)
                {
                    case IteratorState.Start:
                        this.start?.Invoke();
                        this.State = IteratorState.MoveNext; // IteratorState: Start => MoveNext.
                        goto case IteratorState.MoveNext;

                    case IteratorState.MoveNext:
                        if (this.moveNext?.Invoke() ?? false)
                        {
                            this.Current = this.getCurrent != null ? this.getCurrent() : default;
                            return true; // IteratorState: MoveNext => MoveNext.
                        }
                        this.State = IteratorState.End; // IteratorState: MoveNext => End.
                        this.dispose?.Invoke();
                        this.end?.Invoke();
                        break;
                }
                return false;
            }
            catch
            {
                this.State = IteratorState.Error; // IteratorState: Start, MoveNext, End => Error.
                this.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (this.State == IteratorState.Error || this.State == IteratorState.MoveNext)
            {
                try { }
                finally
                {
                    // Unexecuted finally blocks are executed before the thread is aborted.
                    this.State = IteratorState.End; // IteratorState: Error => End.
                    this.dispose?.Invoke();
                }
            }
        }

        public void Reset() => throw new NotSupportedException();
    }

    public class Sequence<T> : IEnumerable<T>
    {
        private readonly Func<Iterator<T>> iteratorFactory;

        public Sequence(Func<Iterator<T>> iteratorFactory) =>
            this.iteratorFactory = iteratorFactory;

        public IEnumerator<T> GetEnumerator() =>
            this.iteratorFactory().Start(); // IteratorState: Create => Start.

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public interface IGenerator<out T> : IEnumerable<T>, IEnumerator<T> { }

    public class Generator<T> : Iterator<T>, IGenerator<T>
    {
        private readonly int initialThreadId = Environment.CurrentManagedThreadId;

        public Generator(
            Action start = null,
            Func<bool> moveNext = null,
            Func<T> getCurrent = null,
            Action dispose = null,
            Action end = null) : base(start, moveNext, getCurrent, dispose, end)
        { }

        public IEnumerator<T> GetEnumerator() =>
            this.initialThreadId == Environment.CurrentManagedThreadId
                && this.State == IteratorState.Create
                // Called by the same initial thread and iteration is not started.
                ? this.Start()
                // If the iteration is already started, or the iteration is requested from a different thread, create new generator with new iterator.
                : new Generator<T>(this.start, this.moveNext, this.getCurrent, this.dispose, this.end).Start();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal static class Generator
    {
        internal static IEnumerable<TSource> RepeatArray<TSource>(TSource value, int count)
        {
            TSource[] results = new TSource[count];
            for (int index = 0; index < count; index++)
            {
                results[index] = value;
            }

            return results;
        }

        internal static IEnumerable<TSource> RepeatSequence<TSource>(
            TSource value, int count) =>
                new Sequence<TSource>(() =>
                {
                    int index = 0;
                    return new Iterator<TSource>(
                        moveNext: () => index++ < count,
                        getCurrent: () => value);
                });

        internal static void CallRepeatSequence<TSource>(TSource value, int count)
        {
            foreach (TSource result in RepeatSequence(value, count)) { }

            // Compiled to:
            using (IEnumerator<TSource> iterator = RepeatSequence(value, count).GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    TSource result = iterator.Current;
                }

                // Virtual control flow inside iterator:
                int index = 0;
                try
                {
                    while (index++ < count)
                    {
                        TSource result = value;
                    }
                }
                finally { }
            }
        }

        internal static IEnumerable<TResult> SelectSequence<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                new Sequence<TResult>(() =>
                {
                    IEnumerator<TSource> sourceIterator = null;
                    return new Iterator<TResult>(
                        start: () => sourceIterator = source.GetEnumerator(),
                        moveNext: () => sourceIterator.MoveNext(),
                        getCurrent: () => selector(sourceIterator.Current),
                        dispose: () => sourceIterator?.Dispose());
                });

        internal static void CallSelectSequence<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TResult result in SelectSequence(source, selector)) { }

            // Compiled to:
            using (IEnumerator<TResult> iterator = SelectSequence(source, selector).GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    TResult result = iterator.Current;
                }

                // Virtual control flow inside iterator:
                IEnumerator<TSource> sourceIterator = null;
                try
                {
                    sourceIterator = source.GetEnumerator();
                    while (sourceIterator.MoveNext())
                    {
                        TResult result = selector(sourceIterator.Current);
                    }
                }
                finally
                {
                    sourceIterator?.Dispose();
                }
            }
        }

        internal static IEnumerable<TSource> WhereSequence<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                new Sequence<TSource>(() =>
                {
                    IEnumerator<TSource> sourceIterator = null;
                    return new Iterator<TSource>(
                        start: () => sourceIterator = source.GetEnumerator(),
                        moveNext: () =>
                        {
                            while (sourceIterator.MoveNext())
                            {
                                if (predicate(sourceIterator.Current))
                                {
                                    return true;
                                }
                            }

                            return false;
                        },
                        getCurrent: () => sourceIterator.Current,
                        dispose: () => sourceIterator?.Dispose());
                });

        internal static void CallWhereSequence<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource result in WhereSequence(source, predicate)) { }

            // Compiled to:
            using (IEnumerator<TSource> iterator = WhereSequence(source, predicate).GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    TSource result = iterator.Current;
                }

                // Virtual control flow inside iterator:
                IEnumerator<TSource> sourceIterator = null;
                try
                {
                    sourceIterator = source.GetEnumerator();
                    while (sourceIterator.MoveNext())
                    {
                        if (predicate(sourceIterator.Current))
                        {
                            TSource result = sourceIterator.Current;
                        }
                    }
                }
                finally
                {
                    sourceIterator?.Dispose();
                }
            }
        }

        internal static IEnumerable<TSource> FromValueSequence<TSource>(TSource value) =>
            new Sequence<TSource>(() =>
                {
                    bool isValueIterated = false;
                    return new Iterator<TSource>(
                        moveNext: () =>
                            {
                                if (!isValueIterated)
                                {
                                    isValueIterated = true;
                                    return true;
                                }

                                return false;
                            },
                        getCurrent: () => value);
                });

        internal static void CallFromValueSequence<TSource>(TSource value)
        {
            foreach (TSource result in FromValueSequence(value)) { }

            // Compiled to:
            using (IEnumerator<TSource> iterator = FromValueSequence(value).GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    TSource result = iterator.Current;
                }

                // Virtual control flow inside iterator:
                bool isValueIterated = false;
                try
                {
                    while (!isValueIterated)
                    {
                        isValueIterated = true;
                        TSource result = value;
                    }
                }
                finally { }
            }
        }

        internal static IEnumerable<TSource> RepeatYield<TSource>(TSource value, int count)
        {
            // Virtual control flow when iterating the results:
            // int index = 0;
            // try
            // {
            //    while (index++ < count)
            //    {
            //        TSource result = value;
            //    }
            // }
            // finally { }

            int index = 0;
            try
            {
                while (index++ < count) // moveNext.
                {
                    yield return value; // getCurrent.
                }
            }
            finally { }
        }

        internal static IEnumerable<TResult> SelectYield<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            IEnumerator<TSource> sourceIterator = null;
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    yield return selector(sourceIterator.Current); // getCurrent.
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }

            // Compiled to:
            // IEnumerator<TSource> sourceIterator = null;
            // return new Generator<TResult>(
            //    start: () => sourceIterator = source.GetEnumerator(),
            //    moveNext: () => sourceIterator.MoveNext(),
            //    getCurrent: () => selector(sourceIterator.Current),
            //    dispose: () => sourceIterator?.Dispose());
        }

        internal static IEnumerable<TSource> WhereYield<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            IEnumerator<TSource> sourceIterator = null;
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    if (predicate(sourceIterator.Current)) // moveNext.
                    {
                        yield return sourceIterator.Current; // getCurrent.
                    }
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }

            // Compiled to:
            // IEnumerator<TSource> sourceIterator = null;
            // return new Generator<TSource>(
            //    start: () => sourceIterator = source.GetEnumerator(),
            //    moveNext: () =>
            //    {
            //        while (sourceIterator.MoveNext())
            //        {
            //            if (predicate(sourceIterator.Current))
            //            {
            //                return true;
            //            }
            //        }
            //
            //        return false;
            //    },
            //    getCurrent: () => sourceIterator.Current,
            //    dispose: () => sourceIterator?.Dispose());
        }

        internal static IEnumerable<TSource> FromValueYield<TSource>(TSource value)
        {
            bool isValueIterated = false;
            try
            {
                while (!isValueIterated) // moveNext.
                {
                    isValueIterated = true; // moveNext.
                    yield return value; // getCurrent.
                }
            }
            finally { }

            // Compiled to:
            // bool isValueIterated = false;
            // return new Generator<TSource>(
            //    moveNext: () =>
            //    {
            //        while (!isValueIterated)
            //        {
            //            isValueIterated = true;
            //            return true;
            //        }
            //
            //        return false;
            //    },
            //    getCurrent: () => value);
        }

        internal static IEnumerable<TSource> Repeat<TSource>(TSource value, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return value;
            }
        }

        internal static IEnumerable<TResult> Select<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }

        internal static IEnumerable<TSource> Where<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }

        internal static IEnumerable<TSource> FromValue<TSource>(TSource value)
        {
            yield return value;
        }

        internal static IEnumerable<TSource> RepeatGenerator<TSource>(TSource value, int count)
        {
            int index = 0;
            return new Generator<TSource>(
                moveNext: () => index++ < count,
                getCurrent: () => value);
        }

        internal static IEnumerator<TSource> RepeatIterator<TSource>(TSource value, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return value;
            }

            // Compiled to:
            // int index = 0;
            // return new Iterator<TSource>(
            //    moveNext: () => index++ < count,
            //    getCurrent: () => value).Start();
        }

        internal static void CallRepeatIterator<TSource>(TSource value, int count)
        {
            using (IEnumerator<TSource> iterator = RepeatIterator(value, count))
            {
                while (iterator.MoveNext())
                {
                    TSource result = iterator.Current;
                }
            }
        }

        internal static IEnumerable<int> YieldBreak()
        {
            yield return 0;
            yield return 1;
            yield return 2;
            yield break;
            yield return 3;
        }

        internal static IEnumerable<TSource> RepeatYieldBreak<TSource>(TSource value, int count)
        {
            int index = 0;
            while (true)
            {
                if (index++ < count)
                {
                    yield return value;
                }
                else
                {
                    yield break;
                }
            }
        }
    }
}
