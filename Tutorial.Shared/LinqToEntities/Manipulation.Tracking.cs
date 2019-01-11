namespace Tutorial.LinqToEntities
{
    using System.Linq;

    using Microsoft.EntityFrameworkCore;

    internal static partial class Tracking
    {
        internal static void EntitiesFromSameDbContext(AdventureWorks adventureWorks)
        {
            Product productById = adventureWorks.Products
                .Single(product => product.ProductID == 999);
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1

            Product productByName = adventureWorks.Products
                .Single(product => product.Name == "Road-750 Black, 52");
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
            object.ReferenceEquals(productById, productByName).WriteLine(); // True
        }

        internal static void ObjectsFromSameDbContext(AdventureWorks adventureWorks)
        {
            var productById = adventureWorks.Products
                .Select(product => new { ProductID = product.ProductID, Name = product.Name })
                .Single(product => product.ProductID == 999);
            var productByName = adventureWorks.Products
                .Select(product => new { ProductID = product.ProductID, Name = product.Name })
                .Single(product => product.Name == "Road-750 Black, 52");
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 0
            object.ReferenceEquals(productById, productByName).WriteLine(); // False
        }

        internal static void EntitiesFromMultipleDbContexts()
        {
            Product productById;
            Product productByName;
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productById = adventureWorks.Products.Single(product => product.ProductID == 999);
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                productByName = adventureWorks.Products.Single(product => product.Name == "Road-750 Black, 52");
            }
            object.ReferenceEquals(productById, productByName).WriteLine(); // False.
        }

        internal static void EntityChanges(AdventureWorks adventureWorks)
        {
            Product create = new Product() { Name = nameof(create), ListPrice = 1 };
            adventureWorks.Products.Add(create); // Create locally.
            Product read = adventureWorks.Products.Single(product => product.ProductID == 999); // Read from remote to local.
            IQueryable<Product> update = adventureWorks.Products
                .Where(product => product.Name.Contains("HL"));
            update.ForEach(product => product.ListPrice += 100); // Update locally.
            IQueryable<Product> delete = adventureWorks.Products
                .Where(product => product.Name.Contains("ML"));
            adventureWorks.Products.RemoveRange(delete); // Delete locally.

            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // True
            adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking =>
            {
                Product changed = tracking.Entity;
                switch (tracking.State)
                {
                    case EntityState.Added:
                    case EntityState.Deleted:
                    case EntityState.Unchanged:
                        $"{tracking.State}: {(changed.ProductID, changed.Name, changed.ListPrice)}".WriteLine();
                        break;
                    case EntityState.Modified:
                        Product original = (Product)tracking.OriginalValues.ToObject();
                        $"{tracking.State}: {(original.ProductID, original.Name, original.ListPrice)} => {(changed.ProductID, changed.Name, changed.ListPrice)}"
                            .WriteLine();
                        break;
                }
            });
            // Added: (-2147482647, toCreate, 1)
            // Unchanged: (999, Road-750 Black, 52, 539.9900)
            // Modified: (951, HL Crankset, 404.9900) => (951, HL Crankset, 504.9900)
            // Modified: (996, HL Bottom Bracket, 121.4900) => (996, HL Bottom Bracket, 221.4900)
            // Deleted: (950, ML Crankset, 256.4900)
            // Deleted: (995, ML Bottom Bracket, 101.2400)
        }

        internal static void Attach(AdventureWorks adventureWorks)
        {
            Product product = new Product() { ProductID = 950, Name = "ML Crankset", ListPrice = 539.99M };
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 0

            adventureWorks.Products.Attach(product);
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 1
            adventureWorks.ChangeTracker.Entries<Product>().Single().State.WriteLine(); // Unchanged
            product.Name = "After attaching";
            adventureWorks.ChangeTracker.Entries<Product>().Single().State.WriteLine(); // Modified
            adventureWorks.ChangeTracker.Entries<Product>().WriteLines(tracking =>
                $"{tracking.State}: {tracking.OriginalValues[nameof(Product.Name)]} => {tracking.CurrentValues[nameof(Product.Name)]}");
            // Modified: ML Crankset => After attaching
        }

        internal static void RelationshipChanges(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories
                .Include(entity => entity.Products).Single(entity => entity.ProductSubcategoryID == 8);
            subcategory.Products.Count.WriteLine(); // 2
            subcategory.Products
                .All(product => product.ProductSubcategory == subcategory).WriteLine(); // True

            subcategory.Products.Clear();
            // Equivalent to: subcategory.Products.ForEach(product => product.ProductSubcategory = null);
            subcategory.Products.Count.WriteLine(); // 0
            subcategory.Products
                .All(product => product.ProductSubcategory == null).WriteLine(); // True
            adventureWorks.ChangeTracker.Entries<Product>().ForEach(tracking =>
            {
                Product original = (Product)tracking.OriginalValues.ToObject();
                Product changed = tracking.Entity;
                $"{tracking.State}: {(original.ProductID, original.Name, original.ProductSubcategoryID)} => {(changed.ProductID, changed.Name, changed.ProductSubcategoryID)}".WriteLine();
            });
            // Modified: (950, ML Crankset, 8) => (950, ML Crankset, )
            // Modified: (951, HL Crankset, 8) => (951, HL Crankset, )
        }

        internal static void AsNoTracking(AdventureWorks adventureWorks)
        {
            Product untracked = adventureWorks.Products.AsNoTracking().First();
            adventureWorks.ChangeTracker.Entries().Count().WriteLine(); // 0
        }

        internal static void DetectChanges(AdventureWorks adventureWorks)
        {
            adventureWorks.ChangeTracker.AutoDetectChangesEnabled = false;
            Product product = adventureWorks.Products.First();
            product.ListPrice += 100;
            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // False
            adventureWorks.ChangeTracker.DetectChanges();
            adventureWorks.ChangeTracker.HasChanges().WriteLine(); // True
        }
    }
}
