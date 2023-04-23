namespace Tutorial.Tests.CategoryTheory
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.CategoryTheory;
    using Tutorial.Tests.LinqToObjects;

    [TestClass]
    public class MonoidTests
    {
        [TestMethod]
        public void StringTest()
        {
            Assert.AreEqual(string.Empty, StringConcatMonoid.Unit);
            Assert.AreEqual("ab", StringConcatMonoid.Multiply("a", "b"));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual("ab", StringConcatMonoid.Multiply(StringConcatMonoid.Unit, "ab"));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual("ab", StringConcatMonoid.Multiply("ab", StringConcatMonoid.Unit));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                StringConcatMonoid.Multiply(StringConcatMonoid.Multiply("a", "b"), "c"),
                StringConcatMonoid.Multiply("a", StringConcatMonoid.Multiply("b", "c")));
        }

        [TestMethod]
        public void Int32Test()
        {
            Assert.AreEqual(0, Int32SumMonoid.Unit);
            Assert.AreEqual(1 + 2, Int32SumMonoid.Multiply(1, 2));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(1, Int32SumMonoid.Multiply(Int32SumMonoid.Unit, 1));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(1, Int32SumMonoid.Multiply(1, Int32SumMonoid.Unit));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(Int32SumMonoid.Multiply(Int32SumMonoid.Multiply(1, 2), 3), Int32SumMonoid.Multiply(1, Int32SumMonoid.Multiply(2, 3)));

            Assert.AreEqual(1, Int32ProductMonoid.Unit);
            Assert.AreEqual(1 * 2, Int32ProductMonoid.Multiply(1, 2));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(2, Int32ProductMonoid.Multiply(Int32ProductMonoid.Unit, 2));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(2, Int32ProductMonoid.Multiply(2, Int32ProductMonoid.Unit));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                Int32ProductMonoid.Multiply(Int32ProductMonoid.Multiply(1, 2), 3),
                Int32ProductMonoid.Multiply(1, Int32ProductMonoid.Multiply(2, 3)));
        }

        [TestMethod]
        public void ClockTest()
        {
            // http://channel9.msdn.com/Shows/Going+Deep/Brian-Beckman-Dont-fear-the-Monads
            Assert.AreEqual(12U, ClockMonoid.Unit);
            Assert.AreEqual((7U + 10U) % 12U, ClockMonoid.Multiply(7U, 10U));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(111U % 12U, ClockMonoid.Multiply(ClockMonoid.Unit, 111U));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(111U % 12U, ClockMonoid.Multiply(111U, ClockMonoid.Unit));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(ClockMonoid.Multiply(ClockMonoid.Multiply(11U, 22U), 33U), ClockMonoid.Multiply(11U, ClockMonoid.Multiply(22U, 33U)));
        }

        [TestMethod]
        public void BooleanTest()
        {
            Assert.IsFalse(BooleanOrMonoid.Unit);
            Assert.AreEqual(true || false, BooleanOrMonoid.Multiply(true, false));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(true, BooleanOrMonoid.Multiply(BooleanOrMonoid.Unit, true));
            Assert.AreEqual(false, BooleanOrMonoid.Multiply(BooleanOrMonoid.Unit, false));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(true, BooleanOrMonoid.Multiply(true, BooleanOrMonoid.Unit));
            Assert.AreEqual(false, BooleanOrMonoid.Multiply(false, BooleanOrMonoid.Unit));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                BooleanOrMonoid.Multiply(BooleanOrMonoid.Multiply(true, false), true),
                BooleanOrMonoid.Multiply(true, BooleanOrMonoid.Multiply(false, true)));

            Assert.IsTrue(BooleanAndMonoid.Unit);
            Assert.AreEqual(true && false, BooleanAndMonoid.Multiply(true, false));

            // Monoid law 1: Unit Binary m == m
            Assert.AreEqual(true, BooleanAndMonoid.Multiply(BooleanAndMonoid.Unit, true));
            Assert.AreEqual(false, BooleanAndMonoid.Multiply(BooleanAndMonoid.Unit, false));

            // Monoid law 2: m Binary Unit == m
            Assert.AreEqual(true, BooleanAndMonoid.Multiply(true, BooleanAndMonoid.Unit));
            Assert.AreEqual(false, BooleanAndMonoid.Multiply(false, BooleanAndMonoid.Unit));

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            Assert.AreEqual(
                BooleanAndMonoid.Multiply(BooleanAndMonoid.Multiply(true, false), true),
                BooleanAndMonoid.Multiply(true, BooleanAndMonoid.Multiply(false, true)));
        }

        [TestMethod]
        public void EnumerableTest()
        {
            Assert.IsFalse(EnumerableConcatMonoid<int>.Unit.Any());
            int[] x = new[] { 0, 1, 2 };
            int[] y = new[] { 3, 4, 5 };
            EnumerableAssert.AreSequentialEqual(EnumerableConcatMonoid<int>.Multiply(x, y), x.Concat(y));

            // Monoid law 1: Unit Binary m == m
            EnumerableAssert.AreSequentialEqual(EnumerableConcatMonoid<int>.Multiply(EnumerableConcatMonoid<int>.Unit, x), x);

            // Monoid law 2: m Binary Unit == m
            EnumerableAssert.AreSequentialEqual(EnumerableConcatMonoid<int>.Multiply(x, EnumerableConcatMonoid<int>.Unit), x);

            // Monoid law 3: (m1 Binary m2) Binary m3 == m1 Binary (m2 Binary m3)
            EnumerableAssert.AreSequentialEqual(
                EnumerableConcatMonoid<int>.Multiply(EnumerableConcatMonoid<int>.Multiply(x, y), x),
                EnumerableConcatMonoid<int>.Multiply(x, EnumerableConcatMonoid<int>.Multiply(y, x)));
        }
    }
}
