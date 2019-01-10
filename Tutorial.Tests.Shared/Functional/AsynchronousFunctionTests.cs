namespace Tutorial.Tests.Functional
{
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Functional;

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
            int result = await AsyncFunctions.AsyncFunctionWithFuncAwaitable(1);
            Assert.AreEqual(1, result);
        }
    }
}
