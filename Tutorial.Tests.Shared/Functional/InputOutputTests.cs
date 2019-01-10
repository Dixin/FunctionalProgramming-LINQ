namespace Tutorial.Tests.Functional
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Functional;

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
