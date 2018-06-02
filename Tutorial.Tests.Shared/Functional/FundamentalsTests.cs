namespace Tutorial.Tests.Functional
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.GettingStarted;

    [TestClass]
    public class FundamentalsTests
    {
        [TestMethod]
        public void TypeTest()
        {
            Basics.ValueTypeReferenceType();
            Basics.Default();
        }
    }
}
