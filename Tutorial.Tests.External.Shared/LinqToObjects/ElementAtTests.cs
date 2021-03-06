// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Xunit;

using Enumerable = Tutorial.LinqToObjects.EnumerableExtensions;
using static Tutorial.LinqToObjects.EnumerableExtensions;

// namespace System.Linq.Tests
namespace Tutorial.Tests.LinqToObjects
{
    public class ElementAtTests : EnumerableTests
    {
        [Fact]
        public void SameResultsRepeatCallsIntQuery()
        {
            var q = from x in new[] { 9999, 0, 888, -1, 66, -777, 1, 2, -12345 }
                    where x > int.MinValue
                    select x;

            Assert.Equal(q.ElementAt(3), q.ElementAt(3));
        }

        [Fact]
        public void SameResultsRepeatCallsStringQuery()
        {
            var q = from x in new[] { "!@#$%^", "C", "AAA", "", "Calling Twice", "SoS", string.Empty }
                    where !string.IsNullOrEmpty(x)
                    select x;

            Assert.Equal(q.ElementAt(4), q.ElementAt(4));
        }

        public static IEnumerable<object[]> TestData()
        {
            yield return new object[] { NumberRangeGuaranteedNotCollectionType(9, 1), 0, 9 };
            yield return new object[] { NumberRangeGuaranteedNotCollectionType(9, 10), 9, 18 };
            yield return new object[] { NumberRangeGuaranteedNotCollectionType(-4, 10), 3, -1 };

            yield return new object[] { new int[] { -4 }, 0, -4 };
            yield return new object[] { new int[] { 9, 8, 0, -5, 10 }, 4, 10 };
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void ElementAt(IEnumerable<int> source, int index, int expected)
        {
            Assert.Equal(expected, source.ElementAt(index));
        }

        [Fact]
        public void InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>("index", () => new int?[] { 9, 8 }.ElementAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => new int[] { 1, 2, 3, 4 }.ElementAt(4));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => new int[0].ElementAt(0));

            Assert.Throws<ArgumentOutOfRangeException>("index", () => NumberRangeGuaranteedNotCollectionType(-4, 5).ElementAt(-1));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => NumberRangeGuaranteedNotCollectionType(5, 5).ElementAt(5));
            Assert.Throws<ArgumentOutOfRangeException>("index", () => NumberRangeGuaranteedNotCollectionType(0, 0).ElementAt(0));

        }

        [Fact]
        public void NullableArray_ValidIndex_ReturnsCorrectObject()
        {
            int?[] source = { 9, 8, null, -5, 10 };

            Assert.Null(source.ElementAt(2));
            Assert.Equal(-5, source.ElementAt(3));
        }

        // [Fact]
        public void NullSource_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null).ElementAt(2));
        }
    }
}
