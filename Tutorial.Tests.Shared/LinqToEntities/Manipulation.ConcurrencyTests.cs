namespace Tutorial.Tests.LinqToEntities
{
    using System.Diagnostics;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    using static TransactionHelper;
    using DbReaderWriter = Tutorial.LinqToEntities.Concurrency.DbReaderWriter;

    [TestClass]
    public class ConcurrencyTests
    {
        [TestMethod]
        public void DetectConflictTest()
        {
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) => Concurrency.NoCheck(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2) =>
            {
                try
                {
                    Concurrency.ConcurrencyCheck(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2));
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
            Rollback((adventureWorks1, adventureWorks2) =>
            {
                try
                {
                    Concurrency.RowVersion(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2));
                    Assert.Fail();
                }
                catch (DbUpdateConcurrencyException exception)
                {
                    Trace.WriteLine(exception);
                }
            });
        }

        [TestMethod]
        public void UpdateConflictTest()
        {
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) =>
                Concurrency.DatabaseWins(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) =>
                Concurrency.ClientWins(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2, adventureWorks3) =>
                Concurrency.MergeClientAndDatabase(new DbReaderWriter(adventureWorks1), new DbReaderWriter(adventureWorks2), new DbReaderWriter(adventureWorks3)));
            Rollback((adventureWorks1, adventureWorks2) =>
                Concurrency.SaveChanges(adventureWorks1, adventureWorks2));
        }
    }
}
