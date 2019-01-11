namespace Tutorial.Tests.LinqToEntities
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tutorial.LinqToEntities;

    [TestClass]
    public class QueryExpressionTests
    {
        [TestMethod]
        public void JoinTest()
        {
            QueryExpressions.InnerJoinWithJoin(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelect(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelectMany(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelectAndRelationship(new AdventureWorks());
            QueryExpressions.InnerJoinWithSelectManyAndRelationship(new AdventureWorks());
            QueryExpressions.LeftOuterJoinWithSelectMany(new AdventureWorks());
        }
    }
}
