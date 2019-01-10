namespace Tutorial.Tests.Functional
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Functional;

    [TestClass]
    public class LocalFunctionTests
    {
        [TestMethod]
        public void BinarySearchTest()
        {
            Assert.AreEqual(-1, Enumerable.Empty<int>().ToArray().BinarySearch(0));
            Assert.AreEqual(-1, Enumerable.Range(0, 10).ToArray().BinarySearch(-1));
            Assert.AreEqual(-1, Enumerable.Range(0, 10).ToArray().BinarySearch(10));
            Enumerable.Range(0, 10).ForEach(int32 => Assert.AreEqual(int32, Enumerable.Range(0, 10).ToArray().BinarySearch(int32)));
        }
    }
}
