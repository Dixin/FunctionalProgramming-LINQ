namespace Tutorial.Tests.Functional
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Functional;

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
