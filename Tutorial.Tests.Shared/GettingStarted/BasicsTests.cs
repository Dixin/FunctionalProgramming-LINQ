namespace Tutorial.Tests.GettingStarted
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.GettingStarted;

    [TestClass]
    public class BasicsTests
    {
        [TestMethod]
        public void TypeTest()
        {
            Basics.ValueTypeReferenceType();
            Basics.Default();
        }
    }
}
