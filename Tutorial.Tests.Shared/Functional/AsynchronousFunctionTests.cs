namespace Tutorial.Tests.Functional
{
    using System.Threading.Tasks;

    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AsyncTests
    {
        [TestMethod]
        public async Task AsyncAwaitTest()
        {
            object value = new object();
            object result = await AsyncFunctions.CompiledAsync(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public async Task FuncAwaitableTest()
        {
            int result = await AsyncFunctions.ReturnFuncAwaitable(1);
            Assert.AreEqual(1, result);
        }
    }
}
