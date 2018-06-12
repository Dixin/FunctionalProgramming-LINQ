namespace Tutorial.Tests.Functional
{
    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DelegatesTests
    {
        [TestMethod]
        public void DelegateTest()
        {
            Delegates.StaticMethod();
        }
    }
}
