namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.LinqToEntities;

    [TestClass]
    public class QueryMethodsTests
    {
        [TestMethod]
        public void FilteringTest()
        {
            QueryMethods.Where(new AdventureWorks());
            QueryMethods.WhereWithOr(new AdventureWorks());
            QueryMethods.WhereWithAnd(new AdventureWorks());
            QueryMethods.WhereAndWhere(new AdventureWorks());
            QueryMethods.WhereWithIs(new AdventureWorks());
            QueryMethods.OfTypeEntity(new AdventureWorks());
            QueryMethods.OfTypePrimitive(new AdventureWorks());
        }

        [TestMethod]
        public void MappingTest()
        {
            QueryMethods.Select(new AdventureWorks());
            QueryMethods.SelectWithStringConcat(new AdventureWorks());
            QueryMethods.SelectEntity(new AdventureWorks());
            QueryMethods.SelectAnonymousType(new AdventureWorks());
        }

        [TestMethod]
        public void GenerationTest()
        {
            QueryMethods.DefaultIfEmptyEntity(new AdventureWorks());
            QueryMethods.DefaultIfEmptyPrimitive(new AdventureWorks());
            QueryMethods.DefaultIfEmptyWithDefaultEntity(new AdventureWorks());
            QueryMethods.DefaultIfEmptyWithDefaultPrimitive(new AdventureWorks());
        }

        [TestMethod]
        public void GroupingTest()
        {
            QueryMethods.GroupBy(new AdventureWorks());
            QueryMethods.GroupByWithElementSelector(new AdventureWorks());
            QueryMethods.GroupByWithResultSelector(new AdventureWorks());
            QueryMethods.GroupByAndSelect(new AdventureWorks());
            QueryMethods.GroupByMultipleKeys(new AdventureWorks());
        }

        [TestMethod]
        public void JoinTest()
        {
            QueryMethods.InnerJoinWithJoin(new AdventureWorks());
            QueryMethods.InnerJoinWithSelect(new AdventureWorks());
            QueryMethods.InnerJoinWithSelectMany(new AdventureWorks());
            QueryMethods.InnerJoinWithSelectAndRelationship(new AdventureWorks());
            QueryMethods.InnerJoinWithSelectManyAndRelationship(new AdventureWorks());
            QueryMethods.InnerJoinWithMultipleKeys(new AdventureWorks());
            QueryMethods.MultipleInnerJoinsWithRelationship(new AdventureWorks());
            QueryMethods.InnerJoinWithGroupJoinAndSelectMany(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithGroupJoin(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithGroupJoinAndSelectMany(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithSelect(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithSelectMany(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithSelectAndRelationship(new AdventureWorks());
            QueryMethods.LeftOuterJoinWithSelectManyAndRelationship(new AdventureWorks());
            QueryMethods.CrossJoinWithSelectMany(new AdventureWorks());
            QueryMethods.CrossJoinWithJoin(new AdventureWorks());
            QueryMethods.SelfJoin(new AdventureWorks());
        }

        [TestMethod]
        public void ConcatenationTest()
        {
            QueryMethods.ConcatPrimitive(new AdventureWorks());
            QueryMethods.ConcatEntity(new AdventureWorks());
        }

        [TestMethod]
        public void SetTest()
        {
            QueryMethods.DistinctEntity(new AdventureWorks());
            QueryMethods.DistinctPrimitive(new AdventureWorks());
            QueryMethods.DistinctMultipleKeys(new AdventureWorks());
            QueryMethods.DistinctWithGroupBy(new AdventureWorks());
            QueryMethods.DistinctEntityWithGroupBy(new AdventureWorks());
            QueryMethods.DistinctMultipleKeysWithGroupBy(new AdventureWorks());
            QueryMethods.DistinctWithGroupByAndFirstOrDefault(new AdventureWorks());
            QueryMethods.UnionEntity(new AdventureWorks());
            QueryMethods.UnionPrimitive(new AdventureWorks());
            QueryMethods.IntersectEntity(new AdventureWorks());
            QueryMethods.IntersectPrimitive(new AdventureWorks());
            QueryMethods.ExceptEntity(new AdventureWorks());
            QueryMethods.ExceptPrimitive(new AdventureWorks());
        }

        [TestMethod]
        public void ConvolutionTest()
        {
            try
            {
                QueryMethods.Zip(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void PartitioningTest()
        {
            QueryMethods.Skip(new AdventureWorks());
            QueryMethods.OrderByAndSkip(new AdventureWorks());
            QueryMethods.Take(new AdventureWorks());
            QueryMethods.SkipAndTake(new AdventureWorks());
            try
            {
                QueryMethods.TakeWhile(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.SkipWhile(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void OrderingTest()
        {
            QueryMethods.OrderBy(new AdventureWorks());
            QueryMethods.OrderByDescending(new AdventureWorks());
            QueryMethods.OrderByAndThenBy(new AdventureWorks());
            try
            {
                QueryMethods.OrderByMultipleKeys(new AdventureWorks());
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.OrderByAndOrderBy(new AdventureWorks());
            try
            {
                QueryMethods.Reverse(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotImplementedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void ConversionTest()
        {
            QueryMethods.CastEntity(new AdventureWorks());
            try
            {
                QueryMethods.CastPrimitive(new AdventureWorks());
                Assert.Fail();
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception);
            }
            QueryMethods.AsEnumerableAsQueryable(new AdventureWorks());
        }

        [TestMethod]
        public void ElementTest()
        {
            QueryMethods.First(new AdventureWorks());
            QueryMethods.FirstOrDefault(new AdventureWorks());
            QueryMethods.Last(new AdventureWorks());
            QueryMethods.LastOrDefault(new AdventureWorks());
            QueryMethods.Single(new AdventureWorks());
            QueryMethods.SingleOrDefault(new AdventureWorks());
            try
            {
                QueryMethods.ElementAt(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
            try
            {
                QueryMethods.ElementAtOrDefault(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }

        [TestMethod]
        public void AggregateTest()
        {
            QueryMethods.Count(new AdventureWorks());
            QueryMethods.LongCount(new AdventureWorks());
            QueryMethods.Max(new AdventureWorks());
            QueryMethods.Min(new AdventureWorks());
            QueryMethods.Sum(new AdventureWorks());
            QueryMethods.Average(new AdventureWorks());
        }

        [TestMethod]
        public void QuantifiersTest()
        {
            QueryMethods.ContainsEntity(new AdventureWorks());
            QueryMethods.ContainsPrimitive(new AdventureWorks());
            QueryMethods.Any(new AdventureWorks());
            QueryMethods.AnyWithPredicate(new AdventureWorks());
            QueryMethods.AllWithPredicate(new AdventureWorks());
            QueryMethods.AllNot(new AdventureWorks());
            QueryMethods.NotAny(new AdventureWorks());
        }

        [TestMethod]
        public void Equality()
        {
            try
            {
                QueryMethods.SequenceEqual(new AdventureWorks());
                Assert.Fail();
            }
            catch (NotSupportedException exception)
            {
                Trace.WriteLine(exception);
            }
        }
    }
}
