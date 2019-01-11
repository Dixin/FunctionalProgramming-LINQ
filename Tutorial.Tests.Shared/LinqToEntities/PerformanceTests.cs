namespace Tutorial.Tests.LinqToEntities
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void PerformanceTest()
        {
            Performance.AddRange();
            Performance.RemoveRange();
        }

        [TestMethod]
        public void CacheTest()
        {
            Performance.CachedEntity(new AdventureWorks());
            Performance.UncachedEntity(new AdventureWorks());
            Performance.Find(new AdventureWorks());
            Performance.TranslationCache(new AdventureWorks());
            Performance.UnreusedTranslationCache(new AdventureWorks());
            Performance.ReusedTranslationCache(new AdventureWorks());
            Performance.CompiledReusedTranslationCache(new AdventureWorks());
            Performance.Translation();
            Performance.UnreusedSkipTakeTranslationCache(new AdventureWorks());
        }

        [TestMethod]
        public async Task AsyncTransactionTest()
        {
            await Performance.DbContextTransactionAsync(new AdventureWorks());
            await Performance.DbTransactionAsync();
        }
    }
}
