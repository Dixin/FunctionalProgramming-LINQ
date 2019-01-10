namespace Tutorial.Tests.Functional
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.Functional;

    [TestClass]
    public class LambdaTests
    {
        [TestMethod]
        public void VisitBodyTest()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            Assert.AreEqual("Add(Subtract(Add(a, b), Divide(Multiply(c, d), 2)), Multiply(e, 3))", expression.PreOrderOutput());
        }

        [TestMethod]
        public void CompileTest()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;

            Func<double, double, double, double, double, double> expected = expression.Compile();
#if !__IOS__
            Func<double, double, double, double, double, double> actual = expression.CompileToCil();
            Assert.AreEqual(expected(1, 2, 3, 4, 5), actual(1, 2, 3, 4, 5));
#endif
        }
    }
}
