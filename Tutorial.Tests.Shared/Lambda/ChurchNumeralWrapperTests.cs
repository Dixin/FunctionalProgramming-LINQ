namespace Tutorial.Tests.LambdaCalculus
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.LambdaCalculus;

    [TestClass]
    public class ChurchNumeralWrapperTests
    {
        [TestMethod]
        public void IncreaseTest()
        {
            NumeralWrapper numeral = 0U.ChurchWrapper();
            Assert.IsTrue(0U + 1U == ++numeral);
            Assert.IsTrue(1U + 1U == ++numeral);
            Assert.IsTrue(2U + 1U == ++numeral);
            Assert.IsTrue(3U + 1U == ++numeral);
            numeral = 123U.ChurchWrapper();
            Assert.IsTrue(123U + 1U == ++numeral);
        }

        [TestMethod]
        public void AddTest()
        {
            Assert.IsTrue(0U + 0U == 0U.ChurchWrapper() + 0U.ChurchWrapper());
            Assert.IsTrue(0U + 1U == 0U.ChurchWrapper() + 1U.ChurchWrapper());
            Assert.IsTrue(10U + 0U == 10U.ChurchWrapper() + 0U.ChurchWrapper());
            Assert.IsTrue(0U + 10U == 0U.ChurchWrapper() + 10U.ChurchWrapper());
            Assert.IsTrue(1U + 1U == 1U.ChurchWrapper() + 1U.ChurchWrapper());
            Assert.IsTrue(10U + 1U == 10U.ChurchWrapper() + 1U.ChurchWrapper());
            Assert.IsTrue(1U + 10U == 1U.ChurchWrapper() + 10U.ChurchWrapper());
            Assert.IsTrue(3U + 5U == 3U.ChurchWrapper() + 5U.ChurchWrapper());
            Assert.IsTrue(123U + 345U == 123U.ChurchWrapper() + 345U.ChurchWrapper());
        }

        [TestMethod]
        public void DecreaseTest()
        {
            NumeralWrapper numeral = 3U.ChurchWrapper();
            Assert.IsTrue(3U - 1U == --numeral);
            Assert.IsTrue(2U - 1U == --numeral);
            Assert.IsTrue(1U - 1U == --numeral);
            Assert.IsTrue(0U == --numeral);
            numeral = 123U.ChurchWrapper();
            Assert.IsTrue(123U - 1U == --numeral);
        }

        [TestMethod]
        public void SubtractTest()
        {
            Assert.IsTrue(0U - 0U == 0U.ChurchWrapper() - 0U.ChurchWrapper());
            Assert.IsTrue(0U == 0U.ChurchWrapper() - 1U.ChurchWrapper());
            Assert.IsTrue(10U - 0U == 10U.ChurchWrapper() - 0U.ChurchWrapper());
            Assert.IsTrue(0U == 0U.ChurchWrapper() - 10U.ChurchWrapper());
            Assert.IsTrue(1U - 1U == 1U.ChurchWrapper() - 1U.ChurchWrapper());
            Assert.IsTrue(10U - 1U == 10U.ChurchWrapper() - 1U.ChurchWrapper());
            Assert.IsTrue(0U == 1U.ChurchWrapper() - 10U.ChurchWrapper());
            Assert.IsTrue(0U == 3U.ChurchWrapper() - 5U.ChurchWrapper());
            Assert.IsTrue(0U == 123U.ChurchWrapper() - 345U.ChurchWrapper());
        }

        [TestMethod]
        public void MultiplyTest()
        {
            Assert.IsTrue(0U * 0U == 0U.ChurchWrapper() * 0U.ChurchWrapper());
            Assert.IsTrue(0U * 1U == 0U.ChurchWrapper() * 1U.ChurchWrapper());
            Assert.IsTrue(10U * 0U == 10U.ChurchWrapper() * 0U.ChurchWrapper());
            Assert.IsTrue(0U * 10U == 0U.ChurchWrapper() * 10U.ChurchWrapper());
            Assert.IsTrue(1U * 1U == 1U.ChurchWrapper() * 1U.ChurchWrapper());
            Assert.IsTrue(10U * 1U == 10U.ChurchWrapper() * 1U.ChurchWrapper());
            Assert.IsTrue(1U * 10U == 1U.ChurchWrapper() * 10U.ChurchWrapper());
            Assert.IsTrue(3U * 5U == 3U.ChurchWrapper() * 5U.ChurchWrapper());
            Assert.IsTrue(12U * 23U == 12U.ChurchWrapper() * 23U.ChurchWrapper());
        }

        [TestMethod]
        public void PowTest()
        {
            Assert.IsTrue((uint)Math.Pow(0U, 1U) == (0U.ChurchWrapper() ^ 1U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(10U, 0U) == (10U.ChurchWrapper() ^ 0U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(0U, 10U) == (0U.ChurchWrapper() ^ 10U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(1U, 1U) == (1U.ChurchWrapper() ^ 1U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(10U, 1U) == (10U.ChurchWrapper() ^ 1U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(1U, 10U) == (1U.ChurchWrapper() ^ 10U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(3U, 5U) == (3U.ChurchWrapper() ^ 5U.ChurchWrapper()));
            Assert.IsTrue((uint)Math.Pow(5U, 3U) == (5U.ChurchWrapper() ^ 3U.ChurchWrapper()));
        }

        [TestMethod]
        public void FactorialTest()
        {
            Func<uint, uint> factorial = null; // Must have to be compiled.
            factorial = x => x == 0 ? 1U : x * factorial(x - 1U);

            Assert.IsTrue(factorial(0U) == 0U.ChurchWrapper().Factorial());
            Assert.IsTrue(factorial(1U) == 1U.ChurchWrapper().Factorial());
            Assert.IsTrue(factorial(2U) == 2U.ChurchWrapper().Factorial());
            Assert.IsTrue(factorial(3U) == 3U.ChurchWrapper().Factorial());
            Assert.IsTrue(factorial(7U) == 7U.ChurchWrapper().Factorial());
        }

        [TestMethod]
        public void FibonacciTest()
        {
            Func<uint, uint> fibonacci = null; // Must have. So that fibonacci can recursively refer itself.
            fibonacci = x => x > 1U ? fibonacci(x - 1) + fibonacci(x - 2) : x;

            Assert.IsTrue(fibonacci(0U) == 0U.ChurchWrapper().Fibonacci());
            Assert.IsTrue(fibonacci(1U) == 1U.ChurchWrapper().Fibonacci());
            Assert.IsTrue(fibonacci(2U) == 2U.ChurchWrapper().Fibonacci());
            Assert.IsTrue(fibonacci(3U) == 3U.ChurchWrapper().Fibonacci());
            Assert.IsTrue(fibonacci(10U) == 10U.ChurchWrapper().Fibonacci());
        }

        [TestMethod]
        public void DivideByTest()
        {
            Assert.IsTrue(1U / 1U == 1U.ChurchWrapper().DivideBy(1U.ChurchWrapper()));
            Assert.IsTrue(1U / 2U == 1U.ChurchWrapper().DivideBy(2U.ChurchWrapper()));
            Assert.IsTrue(2U / 2U == 2U.ChurchWrapper().DivideBy(2U.ChurchWrapper()));
            Assert.IsTrue(2U / 1U == 2U.ChurchWrapper().DivideBy(1U.ChurchWrapper()));
            Assert.IsTrue(10U / 3U == 10U.ChurchWrapper().DivideBy(3U.ChurchWrapper()));
            Assert.IsTrue(3U / 10U == 3U.ChurchWrapper().DivideBy(10U.ChurchWrapper()));
        }
    }
}
