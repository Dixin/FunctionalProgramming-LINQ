namespace Tutorial.Tests.LinqToEntities
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    [TestClass]
    public class TracingTests
    {
        [TestMethod]
        public void TracingTest()
        {
            Tracing.Logger();
            Tracing.TagWith(new AdventureWorks());
        }
    }
}
