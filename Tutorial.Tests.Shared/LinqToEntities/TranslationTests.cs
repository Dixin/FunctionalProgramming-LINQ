namespace Tutorial.Tests.LinqToEntities
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToEntities;

    [TestClass]
    public class TranslationTests
    {
        [TestMethod]
        public void WhereAndSelectTest()
        {
            Translation.WhereAndSelect(new AdventureWorks());
            Translation.WhereAndSelectLinqExpressions(new AdventureWorks());
            Translation.CompileWhereAndSelectExpressions(new AdventureWorks());
            Translation.WhereAndSelectDatabaseExpressions(new AdventureWorks());
            Translation.WhereAndSelectSql(new AdventureWorks());
        }

        [TestMethod]
        public void SelectAndFirstTest()
        {
            Translation.SelectAndFirst(new AdventureWorks());
            Translation.SelectAndFirstLinqExpressions(new AdventureWorks());
            Translation.CompileSelectAndFirstExpressions(new AdventureWorks());
            Translation.SelectAndFirstDatabaseExpressions(new AdventureWorks());
            Translation.SelectAndFirstSql(new AdventureWorks());
        }

        [TestMethod]
        public void ApiTranslationTest()
        {
            Translation.WhereAndSelectWithCustomPredicate(new AdventureWorks());
            Translation.WhereAndSelectWithLocalPredicate(new AdventureWorks());
        }
    }
}
