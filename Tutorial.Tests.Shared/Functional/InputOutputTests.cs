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
            InputOutput.CallInputByCopy();
            InputOutput.CallInputByAlias();
            InputOutput.CallOutputParameter();
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
            InputOutput.OutputByCopy();
            InputOutput.OutputByAlias();
        }
    }
}
