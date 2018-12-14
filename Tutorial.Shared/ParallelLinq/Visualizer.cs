namespace Tutorial.ParallelLinq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Microsoft.ConcurrencyVisualizer.Instrumentation;

#if DEMO
    public partial class Markers
    {
        public static Span EnterSpan(int category, string spanName) =>
            new Span(category, spanName);

        public static MarkerSeries CreateMarkerSeries(string markSeriesName) =>
            new MarkerSeries(markSeriesName);
    }

    public class Span : IDisposable
    {
        private readonly int category;

        private readonly string spanName;

        private readonly DateTime start;

        public Span(int category, string spanName, string markSeriesName = null)
        {
            this.category = category;
            this.spanName = string.IsNullOrEmpty(markSeriesName) ? spanName : $"{markSeriesName}/{spanName}";
            this.start = DateTime.Now;
            $"{this.start.ToString("o")}: thread id: {Thread.CurrentThread.ManagedThreadId}, category: {this.category}, span: {this.spanName}".WriteLine();
        }

        public void Dispose()
        {
            DateTime end = DateTime.Now;
            $"{end.ToString("o")}: thread id: {Thread.CurrentThread.ManagedThreadId}, category: {this.category}, span: {this.spanName}, duration: {end – this.start}".WriteLine();
        }
    }

    public class MarkerSeries
    {
        private readonly string markSeriesName;

        public MarkerSeries(string markSeriesName) => this.markSeriesName = markSeriesName;

        public Span EnterSpan(int category, string spanName) =>
            new Span(category, spanName, this.markSeriesName);
    }

    public partial class Markers
    {
        static Markers()
        {
            // Trace to file:
            Trace.Listeners.Add(new TextWriterTraceListener(@"D:\Temp\Trace.txt"));
            // Trace to console:
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }
    }
#endif

    public static partial class Visualizer
    {
        internal const string ParallelSpan = "Parallel";

        internal const string SequentialSpan = "Sequential";

        internal static void Visualize<TSource>(
            this IEnumerable<TSource> source,
            Action<IEnumerable<TSource>, Action<TSource>> query,
            Action<TSource> iteratee, string span = SequentialSpan, int category = -1)
        {
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                query(
                    source,
                    value =>
                    {
                        using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                        {
                            iteratee(value);
                        }
                    });
            }
        }

        internal static void Visualize<TSource>(
            this ParallelQuery<TSource> source,
            Action<ParallelQuery<TSource>, Action<TSource>> query,
            Action<TSource> iteratee, string span = ParallelSpan, int category = -2)
        {
            using (Markers.EnterSpan(category, span))
            {
                MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
                query(
                    source,
                    value =>
                    {
                        using (markerSeries.EnterSpan(Thread.CurrentThread.ManagedThreadId, value.ToString()))
                        {
                            iteratee(value);
                        }
                    });
            }
        }

        internal static int ComputingWorkload(int value = 0, int baseIteration = 10_000_000)
        {
            for (int i = 0; i < baseIteration * (value + 1); i++) { }
            return value;
        }

        internal static TResult Visualize<TSource, TMiddle, TResult>(
            this IEnumerable<TSource> source,
            Func<IEnumerable<TSource>, Func<TSource, TMiddle>, TResult> query,
            Func<TSource, TMiddle> iteratee,
            Func<TSource, string> spanFactory = null,
            string span = SequentialSpan)
        {
            spanFactory = spanFactory ?? (value => value.ToString());
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return query(
                source,
                value =>
                {
                    using (markerSeries.EnterSpan(
                        Thread.CurrentThread.ManagedThreadId, spanFactory(value)))
                    {
                        return iteratee(value);
                    }
                });
        }

        internal static TResult Visualize<TSource, TMiddle, TResult>(
            this ParallelQuery<TSource> source,
            Func<ParallelQuery<TSource>, Func<TSource, TMiddle>, TResult> query,
            Func<TSource, TMiddle> iteratee,
            Func<TSource, string> spanFactory = null,
            string span = ParallelSpan)
        {
            spanFactory = spanFactory ?? (value => value.ToString());
            MarkerSeries markerSeries = Markers.CreateMarkerSeries(span);
            return query(
                source,
                value =>
                {
                    using (markerSeries.EnterSpan(
                        Thread.CurrentThread.ManagedThreadId, spanFactory(value)))
                    {
                        return iteratee(value);
                    }
                });
        }
    }
}
