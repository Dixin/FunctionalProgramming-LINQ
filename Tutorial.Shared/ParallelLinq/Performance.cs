namespace Tutorial.ParallelLinq
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Xml.Linq;

    using static Visualizer;

    using EnumerableX = Tutorial.LinqToObjects.EnumerableX;
    using Stopwatch = System.Diagnostics.Stopwatch;

    internal static partial class Performance
    {
        internal static void OrderByTest(Func<int, int> keySelector, int sourceCount, int testRepeatCount)
        {
            int[] source = EnumerableX
                .RandomInt32(min: int.MinValue, max: int.MaxValue, count: sourceCount)
                .ToArray();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Enumerable.Range(0, testRepeatCount).ForEach(_ =>
            {
                int[] sequentialResults = source.OrderBy(keySelector).ToArray();
            });
            stopwatch.Stop();
            $"Sequential:{stopwatch.ElapsedMilliseconds}".WriteLine();

            stopwatch.Restart();
            Enumerable.Range(0, testRepeatCount).ForEach(_ =>
            {
                int[] parallel1Results = source.AsParallel().OrderBy(keySelector).ToArray();
            });
            stopwatch.Stop();
            $"Parallel:{stopwatch.ElapsedMilliseconds}".WriteLine();
        }

        internal static void OrderByTestForSourceCount()
        {
            OrderByTest(keySelector: value => value, sourceCount: 5, testRepeatCount: 10_000);
            // Sequential:11    Parallel:1422
            OrderByTest(keySelector: value => value, sourceCount: 5_000, testRepeatCount: 100);
            // Sequential:114   Parallel:107
            OrderByTest(keySelector: value => value, sourceCount: 500_000, testRepeatCount: 100);
            // Sequential:18210 Parallel:8204
        }

        internal static void OrderByTestForKeySelector()
        {
            OrderByTest(
                keySelector: value => value + ComputingWorkload(baseIteration: 1),
                sourceCount: Environment.ProcessorCount, testRepeatCount: 100_000);
            // Sequential:37   Parallel:2218
            OrderByTest(
                keySelector: value => value + ComputingWorkload(baseIteration: 10_000),
                sourceCount: Environment.ProcessorCount, testRepeatCount: 1_000);
            // Sequential:115  Parallel:125
            OrderByTest(
                keySelector: value => value + ComputingWorkload(baseIteration: 100_000),
                sourceCount: Environment.ProcessorCount, testRepeatCount: 100);
            // Sequential:1240 Parallel:555
        }

        internal static void ForceParallel<TSource>(
            this IEnumerable<TSource> source, Action<TSource> iteratee, int degreeOfParallelism)
        {
            if (degreeOfParallelism <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism));
            }

            IList<IEnumerator<TSource>> partitions = Partitioner
                .Create(source, EnumerablePartitionerOptions.NoBuffering) // Stripped partitioning.
                .GetPartitions(degreeOfParallelism);
            ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception>();
            void Run(IEnumerator<TSource> partition)
            {
                try
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            iteratee(partition.Current);
                        }
                    }
                }
                catch (Exception exception)
                {
                    exceptions.Add(exception);
                }
            }

            Thread[] threads = partitions
                .Skip(1)
                .Select(partition => new Thread(() => Run(partition)))
                .ToArray();
            threads.ForEach(thread => thread.Start());
            Run(partitions[0]);
            threads.ForEach(thread => thread.Join());
            if (!exceptions.IsEmpty)
            {
                throw new AggregateException(exceptions);
            }
        }

        internal static void DownloadTest(string[] uris)
        {
            byte[] Download(string uri)
            {
                using (WebClient webClient = new WebClient())
                {
                    return webClient.DownloadData(uri);
                }
            }

            uris.Visualize(EnumerableEx.ForEach, uri => Download(uri).Length.WriteLine());

            const int DegreeOfParallelism = 10;
            uris.AsParallel()
                .WithDegreeOfParallelism(DegreeOfParallelism)
                .Visualize(ParallelEnumerable.ForAll, uri => Download(uri).Length.WriteLine());

            uris.Visualize(
                query: (source, iteratee) => source.ForceParallel(iteratee, DegreeOfParallelism),
                iteratee: uri => Download(uri).Length.WriteLine());
        }

        internal static void RunDownloadTestWithSmallFiles()
        {
            string[] thumbnailUris =
                XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Descendants((XNamespace)"http://search.yahoo.com/mrss/" + "thumbnail")
                .Attributes("url")
                .Select(uri => (string)uri)
                .ToArray();
            DownloadTest(thumbnailUris);
        }

        internal static void RunDownloadTestWithLargeFiles()
        {
            string[] pictureUris =
                XDocument.Load("https://www.flickr.com/services/feeds/photos_public.gne?id=64715861@N07&format=rss2")
                .Descendants((XNamespace)"http://search.yahoo.com/mrss/" + "content")
                .Attributes("url")
                .Select(uri => (string)uri)
                .ToArray();
            DownloadTest(pictureUris);
        }
    }
}
