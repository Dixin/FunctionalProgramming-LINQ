namespace Tutorial.LinqToEntities
{
    internal static partial class UnitOfWork
    {
        internal static void Dispose()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                // Unit of work.
            }
        }
    }
}
