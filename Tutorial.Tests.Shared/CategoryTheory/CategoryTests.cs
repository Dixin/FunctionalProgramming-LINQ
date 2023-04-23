namespace Tutorial.Tests.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.CategoryTheory;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class CategoryTests
    {
        [TestMethod]
        public void DotNetCategoryObjectsTest()
        {
            IEnumerable<Type> types = DotNetCategory.Objects;
            EnumerableAssert.Multiple(types);
        }

        [TestMethod]
        public void DotNetCategoryComposeTest()
        {
            Func<int, double> function1 = int32 => Math.Sqrt(int32);
            Func<double, string> function2 = @double => @double.ToString("0.00");
            Delegate function = DotNetCategory.Compose(function2, function1);
            Assert.AreEqual("1.41", function.DynamicInvoke(2));
        }
    }
}
