namespace Tutorial.Tests.Functional
{
    using Tutorial.Functional;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParametersTests
    {
        [TestMethod]
        public void ParameterTest()
        {
            InputOutput.CallPassByValue();
            InputOutput.CallPassByReference();
            InputOutput.CallOutput();
            InputOutput.OutVariable();
        }

        [TestMethod]
        public void CallerInfoTest()
        {
            InputOutput.CallTraceWithCaller();
        }

        [TestMethod]
        public void ReturnTest()
        {
            InputOutput.ReturnByValue();
            InputOutput.ReturnByReference();
        }
    }
}
