namespace Tutorial.Tests.LinqToObjects
{
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.LinqToObjects;

    [TestClass]
    public class DeferredExecutionTests
    {
        [TestMethod]
        public void ReverseTest()
        {
            int[] enumerable = new int[] { 0, 1, 2 };
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Reverse(enumerable),
                DeferredExecution.CompiledReverseGenerator(enumerable));

            enumerable = Array.Empty<int>();
            EnumerableAssert.AreSequentialEqual(
                Enumerable.Reverse(enumerable),
                DeferredExecution.CompiledReverseGenerator(enumerable));
        }

        [TestMethod]
        public void QueryTest()
        {
            DeferredExecution.ForEachWhereAndSelect();
            DeferredExecution.ForEachSelectAndReverse();
        }
    }
}