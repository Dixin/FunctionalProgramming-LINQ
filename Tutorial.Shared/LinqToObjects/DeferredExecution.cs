namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class DeferredExecution
    {
        internal static IEnumerable<double> AbsAndSqrtArray(double @double) =>
            new double[]
            {
                Math.Abs(@double),
                Math.Sqrt(@double)
            };

        internal static IEnumerable<double> AbsAndSqrtGenerator(double @double)
        {
            yield return Math.Abs(@double);
            yield return Math.Sqrt(@double);
        }

        internal static IEnumerable<TResult> SelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            "Select query starts.".WriteLine();
            foreach (TSource value in source)
            {
                $"Select query calls selector with {value}.".WriteLine();
                TResult result = selector(value);
                $"Select query yields {result}.".WriteLine();
                yield return result;
            }
            "Select query ends.".WriteLine();
        }

        internal static IEnumerable<TSource> WhereGenerator<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            "Where query starts.".WriteLine();
            foreach (TSource value in source)
            {
                $"Where query calls predicate with {value}.".WriteLine();
                if (predicate(value))
                {
                    $"Where query yields {value}.".WriteLine();
                    yield return value;
                }
            }
            "Where query ends.".WriteLine();
        }

        internal static IEnumerable<TResult> DesugaredSelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            "Select query starts.".WriteLine();
            IEnumerator<TSource> sourceIterator = null; // start.
            try
            {
                sourceIterator = source.GetEnumerator(); // start.
                while (sourceIterator.MoveNext()) // moveNext.
                {
                    $"Select query calls selector with {sourceIterator.Current}.".WriteLine(); // getCurrent.
                    TResult result = selector(sourceIterator.Current); // getCurrent.
                    $"Select query yields {result}.".WriteLine(); // getCurrent.
                    yield return result; // getCurrent.
                }
            }
            finally
            {
                sourceIterator?.Dispose(); // dispose.
            }
            "Select query ends.".WriteLine(); // end.
        }

        internal static IEnumerable<TResult> CompiledSelectGenerator<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            IEnumerator<TSource> sourceIterator = null;
            return new Generator<TResult>(
                start: () =>
                {
                    "Select query starts.".WriteLine();
                    sourceIterator = source.GetEnumerator();
                },
                moveNext: () => sourceIterator.MoveNext(),
                getCurrent: () =>
                {
                    $"Select query calls selector with {sourceIterator.Current}.".WriteLine();
                    TResult result = selector(sourceIterator.Current);
                    $"Select query yields {result}.".WriteLine();
                    return result;
                },
                dispose: () => sourceIterator?.Dispose(),
                end: () => "Select query ends.".WriteLine());
        }

        internal static IEnumerable<TSource> CompiledWhereGenerator<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            IEnumerator<TSource> sourceIterator = null;
            return new Generator<TSource>(
                start: () =>
                {
                    "Where query starts.".WriteLine();
                    sourceIterator = source.GetEnumerator();
                },
                moveNext: () =>
                {
                    while (sourceIterator.MoveNext())
                    {
                        $"Where query calls predicate with {sourceIterator.Current}."
                            .WriteLine();
                        if (predicate(sourceIterator.Current))
                        {
                            return true;
                        }
                    }

                    return false;
                },
                getCurrent: () =>
                {
                    $"Where query yields {sourceIterator.Current}.".WriteLine();
                    return sourceIterator.Current;
                },
                dispose: () => sourceIterator?.Dispose(),
                end: () => "Where query ends.".WriteLine());
        }

        internal static void CallWhereAndSelect()
        {
            IEnumerable<int> source = Enumerable.Range(1, 5);
            IEnumerable<string> deferredQuery = source
                .WhereGenerator(int32 => int32 > 2) // Deferred execution.
                .SelectGenerator(int32 => new string('*', int32)); // Deferred execution.
            foreach (string result in deferredQuery)
            {
                // Select query starts.
                // Where query starts.
                // Where query calls predicate with 1.
                // Where query calls predicate with 2.
                // Where query calls predicate with 3.
                // Where query yields 3.
                // Select query calls selector with 3.
                // Select query yields ***.
                // Where query calls predicate with 4.
                // Where query yields 4.
                // Select query calls selector with 4.
                // Select query yields ****.
                // Where query calls predicate with 5.
                // Where query yields 5.
                // Select query calls selector with 5.
                // Select query yields *****.
                // Where query ends.
                // Select query ends.
            }
        }

        internal static IEnumerable<TSource> ReverseGenerator<TSource>(this IEnumerable<TSource> source)
        {
            "Reverse query starts.".WriteLine();
            TSource[] results = source.ToArray();
            $"Reverse query has all {results.Length} value(s) of source sequence.".WriteLine();
            for (int index = results.Length - 1; index >= 0; index--)
            {
                $"Reverse query yields index {index} of source sequence.".WriteLine();
                yield return results[index];
            }
            "Reverse query ends.".WriteLine();
        }

        internal static IEnumerable<TSource> CompiledReverseGenerator<TSource>(this IEnumerable<TSource> source)
        {
            TSource[] results = null;
            int index = 0;
            return new Generator<TSource>(
                start: () =>
                {
                    "Reverse query starts.".WriteLine();
                    results = source.ToArray();
                    $"Reverse query evaluated all {results.Length} value(s) of source sequence.".WriteLine();
                    index = results.Length - 1;
                },
                moveNext: () => index >= 0,
                getCurrent: () =>
                {
                    $"Reverse query yields index {index} of source source.".WriteLine();
                    return results[index--];
                },
                end: () => "Reverse query ends.".WriteLine());
        }

        internal static void CallSelectAndReverse()
        {
            IEnumerable<string> deferredQuery = Enumerable.Range(1, 5)
                .SelectGenerator(int32 => new string('*', int32)) // Deferred execution.
                .ReverseGenerator(); // Deferred execution.
            foreach (string result in deferredQuery)
            {
                // Reverse query starts.
                // Select query starts.
                // Select query calls selector with 1.
                // Select query yields *.
                // Select query calls selector with 2.
                // Select query yields **.
                // Select query calls selector with 3.
                // Select query yields ***.
                // Select query calls selector with 4.
                // Select query yields ****.
                // Select query calls selector with 5.
                // Select query yields *****.
                // Select query ends.
                // Reverse query has all 5 value(s) of source sequence.
                // Reverse query yields index 4 of source sequence.
                // Reverse query yields index 3 of source sequence.
                // Reverse query yields index 2 of source sequence.
                // Reverse query yields index 1 of source sequence.
                // Reverse query yields index 0 of source sequence.
                // Reverse query ends.
            }
        }

        internal static IEnumerable<TResult> SelectWithDeferredCheck<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            foreach (TSource value in source)
            {
                yield return selector(value); // Deferred execution.
            }
        }

        internal static IEnumerable<TResult> CompiledSelectWithDeferredCheck<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            IEnumerator<TSource> sourceIterator = null;
            return new Generator<TResult>(
                start: () =>
                {
                    if (source == null)
                    {
                        throw new ArgumentNullException(nameof(source));
                    }

                    if (selector == null)
                    {
                        throw new ArgumentNullException(nameof(selector));
                    }

                    sourceIterator = source.GetEnumerator();
                },
                moveNext: () => sourceIterator.MoveNext(),
                getCurrent: () => selector(sourceIterator.Current),
                dispose: () => sourceIterator?.Dispose());
        }

        internal static IEnumerable<TResult> SelectWithCheck<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null) // Immediate execution.
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (selector == null) // Immediate execution.
            {
                throw new ArgumentNullException(nameof(selector));
            }

            IEnumerable<TResult> SelectGenerator()
            {
                foreach (TSource value in source)
                {
                    yield return selector(value); // Deferred execution.
                }
            }
            return SelectGenerator(); // Immediate execution.
        }
    }
}
