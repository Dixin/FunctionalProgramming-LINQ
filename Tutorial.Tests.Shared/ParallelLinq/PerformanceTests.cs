namespace Tutorial.Tests.ParallelLinq
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.ParallelLinq;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void ComputingTest()
        {
            Performance.OrderByTestForSourceCount();
            Performance.OrderByTestForKeySelector();
        }

        [TestMethod]
        public void IOTest()
        {
            Performance.RunDownloadTestWithSmallFiles();
            Performance.RunDownloadTestWithLargeFiles();
        }

        [TestMethod]
        public void ForceParallelTest()
        {
            ConcurrentBag<int> threadIds = new ConcurrentBag<int>();
            int forcedDegreeOfParallelism = 5;
            Enumerable.Range(0, forcedDegreeOfParallelism * 10).ForceParallel(
                value => threadIds.Add(Thread.CurrentThread.ManagedThreadId + Visualizer.ComputingWorkload()),
                forcedDegreeOfParallelism);
            Assert.AreEqual(forcedDegreeOfParallelism, threadIds.Distinct().Count());
        }
    }
}
