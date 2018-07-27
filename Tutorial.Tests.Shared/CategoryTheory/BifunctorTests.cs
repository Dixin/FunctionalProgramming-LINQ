namespace Tutorial.Tests.CategoryTheory
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.CategoryTheory;

    [TestClass]
    public class BifunctorTests
    {
        [TestMethod]
        public void LazyTest()
        {
            bool isExecuted1 = false;
            bool isExecuted2 = false;
            Tutorial.CategoryTheory.Lazy<int, string> lazyBinaryFunctor = new Tutorial.CategoryTheory.Lazy<int, string>(() => (1, "abc"));
            Func<int, bool> selector1 = x => { isExecuted1 = true; return x > 0; };
            Func<string, int> selector2 = x => { isExecuted2 = true; return x.Length; };

            Tutorial.CategoryTheory.Lazy<bool, int> query = lazyBinaryFunctor.Select(selector1, selector2);
            Assert.IsFalse(isExecuted1); // Deferred and lazy.
            Assert.IsFalse(isExecuted2); // Deferred and lazy.

            Assert.AreEqual(true, query.Value1); // Execution.
            Assert.AreEqual("abc".Length, query.Value2); // Execution.
            Assert.IsTrue(isExecuted1);
            Assert.IsTrue(isExecuted2);
        }
    }
}