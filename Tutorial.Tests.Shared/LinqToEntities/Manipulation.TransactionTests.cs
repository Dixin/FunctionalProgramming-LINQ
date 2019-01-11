namespace Tutorial.Tests.LinqToEntities
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void TransactionTest()
        {
            Transactions.Default(new AdventureWorks());
            Transactions.DbContextTransaction(new AdventureWorks());
            Transactions.DbTransaction();
            Transactions.TransactionScope(new AdventureWorks());
        }
    }
}
