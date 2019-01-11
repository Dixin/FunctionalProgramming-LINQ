namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.LinqToEntities;

    [TestClass]
    public class BinaryArithmeticTranslatorTests
    {
        [TestMethod]
        public void TranslateToSql()
        {
            RemoteQueries.Infix();

            Expression<Func<double, double, double>> expression1 = (a, b) => a * a + b * b;
            Assert.AreEqual("SELECT ((@a * @a) + (@b * @b));", expression1.InOrder());

            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            Assert.AreEqual("SELECT (((@a + @b) - ((@c * @d) / 2)) + (@e * 3));", expression2.InOrder());
        }

        [TestMethod]
        public void ExecuteSql()
        {
            RemoteQueries.TranslateAndExecute();

            Expression<Func<double, double, double>> expression1 = (a, b) => a * a + b * b;
            Func<double, double, double> local1 = expression1.Compile();
#if !__IOS__
            Func<double, double, double> remote1 = expression1.TranslateToSql(ConnectionStrings.AdventureWorks);
            Assert.AreEqual(local1(1, 2), remote1(1, 2));
#endif

            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            Func<double, double, double, double, double, double> local2 = expression2.Compile();
#if !__IOS__
            Func<double, double, double, double, double, double> remote2 = expression2.TranslateToSql(ConnectionStrings.AdventureWorks);
            Assert.AreEqual(local2(1, 2, 3, 4, 5), remote2(1, 2, 3, 4, 5));
#endif
        }
    }
}
