namespace Tutorial.Tests.Functional
{
    using System;
    using System.Diagnostics;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.GettingStarted;

    [TestClass]
    public class GenericTests
    {
        [TestMethod]
        public void StackTest()
        {
            Stack<int> stack = new Stack<int>();
            stack.Push(1);
            Assert.AreEqual(1, stack.Pop());
            stack.Push(2);
            stack.Push(3);
            Assert.AreEqual(3, stack.Pop());
            stack.Push(4);
            Assert.AreEqual(4, stack.Pop());
            Assert.AreEqual(2, stack.Pop());
            try
            {
                stack.Pop();
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
        }
    }
}
