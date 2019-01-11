namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    [TestClass]
    public class LoadingTests
    {
        [TestMethod]
        public void DisposableTest()
        {
            try
            {
                Loading.UI.RenderCategoryProducts("Bikes");
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void DeferredExecutionTest()
        {
            Loading.DeferredExecution(new AdventureWorks());
        }

        [TestMethod]
        public void ExplicitLoadingTest()
        {
            Loading.ExplicitLoading(new AdventureWorks());
            Loading.ExplicitLoadingWithQuery(new AdventureWorks());
        }

        [TestMethod]
        public void LazyLoadingTest()
        {
            Loading.LazyLoading(new AdventureWorks());
            Loading.MultipleLazyLoading(new AdventureWorks());
        }

        [TestMethod]
        public void EagerLoadingTest()
        {
            Loading.EagerLoadingWithInclude(new AdventureWorks());
            Loading.EagerLoadingMultipleLevels(new AdventureWorks());
            Loading.EagerLoadingWithSelect(new AdventureWorks());
            try
            {
                Loading.ConditionalEagerLoadingWithInclude(new AdventureWorks());
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            Loading.ConditionalEagerLoadingWithJoin(new AdventureWorks());
        }

        [TestMethod]
        public void DisableLazyLoadingTest()
        {
            Loading.DisableLazyLoading();
        }
    }
}
