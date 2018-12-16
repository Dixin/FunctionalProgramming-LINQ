namespace Tutorial.Tests.ParallelLinq
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.ParallelLinq;

    [TestClass]
    public partial class QueryMethodsTests
    {
        [TestMethod]
        public void QueryTest()
        {
            QueryMethods.Generation();
            QueryMethods.AsParallelAsSequential();
            QueryMethods.QueryExpression();
            QueryMethods.ForEachForAll();
            QueryMethods.RenderForEachForAllSpans();
            QueryMethods.VisualizeForEachForAll();
            QueryMethods.VisualizeWhereSelect();
        }

        [TestMethod]
        public void CancelTest()
        {
            QueryMethods.Cancel();
        }

        [TestMethod]
        public void DegreeOfParallelismTest()
        {
            QueryMethods.DegreeOfParallelism();
        }

        [TestMethod]
        public void ExecutionModeTest()
        {
            QueryMethods.ExecutionMode();
        }

        [TestMethod]
        public void MergeTest()
        {
            QueryMethods.MergeForSelect();
            QueryMethods.MergeForOrderBy();
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.CommutativeAssociative();
            QueryMethods.AggregateCorrectness();
            QueryMethods.VisualizeAggregate();
            QueryMethods.MergeForAggregate();
        }
        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.AsOrdered();
            QueryMethods.AsUnordered();
            QueryMethods.OrderBy();
            QueryMethods.Correctness();
        }

        [TestMethod]
        public void PartitionerTest()
        {
            try
            {
                Partitioning.QueryOrderablePartitioner();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
        }
    }
}
