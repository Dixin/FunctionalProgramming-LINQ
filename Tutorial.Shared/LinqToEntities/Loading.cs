namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query.Expressions;
    using Microsoft.EntityFrameworkCore.Storage;

    public static class QueryableExtensions
    {
        public static IEnumerator<TEntity> GetEntityIterator<TEntity>(
            this IQueryable<TEntity> query, DbContext dbContext) where TEntity : class
        {
            "| |_Compile LINQ expression tree to database expression tree.".WriteLine();
            (SelectExpression DatabaseExpression, IReadOnlyDictionary<string, object> Parameters) compilation =
                dbContext.Compile(query.Expression);

            IEnumerator<TEntity> entityIterator = null;
            return new Tutorial.LinqToObjects.Iterator<TEntity>(
                start: () =>
                {
                    "| |_Generate SQL from database expression tree.".WriteLine();
                    IRelationalCommand sql = dbContext.Generate(
                        compilation.DatabaseExpression, compilation.Parameters);
                    IEnumerable<TEntity> sqlQuery = dbContext.Set<TEntity>().FromSql(
                        sql: sql.CommandText,
                        parameters: compilation.Parameters
                            .Select(parameter => new SqlParameter(parameter.Key, parameter.Value)).ToArray());
                    entityIterator = sqlQuery.GetEnumerator();
                    "| |_Execute generated SQL.".WriteLine();
                },
                moveNext: () => entityIterator.MoveNext(),
                getCurrent: () =>
                {
                    $"| |_Materialize data row to {typeof(TEntity).Name} entity.".WriteLine();
                    return entityIterator.Current;
                },
                dispose: () => entityIterator.Dispose(),
                end: () => "  |_End.".WriteLine()).Start();
        }
    }

    internal static partial class Loading
    {
        internal static void DeferredExecution(AdventureWorks adventureWorks)
        {
            IQueryable<Product> categories = adventureWorks.Products
                .Where(product => product.Name.Length > 100)
                .Take(3);
            "Iterator - Create from LINQ to Entities query.".WriteLine();
            using (IEnumerator<Product> iterator = categories.GetEntityIterator(adventureWorks)) // Compile query.
            {
                int index = 0;
                while (new Func<bool>(() =>
                    {
                        bool moveNext = iterator.MoveNext();
                        $"|_Iterator - [{index++}] {nameof(IEnumerator<Product>.MoveNext)}: {moveNext}.".WriteLine();
                        return moveNext; // Generate SQL when first time called.
                    })())
                {
                    Product product = iterator.Current;
                    $"| |_Iterator - [{index}] {nameof(IEnumerator<Product>.Current)}: {product.Name}.".WriteLine();
                }
            }
            // Iterator - Create from LINQ to Entities query.
            // | |_Compile LINQ expression tree to database expression tree.
            // |_Iterator - [0] MoveNext: True.
            // | |_Generate SQL from database expression tree.
            // | |_Execute generated SQL.
            // | |_Materialize data row to Product entity.
            // | |_Iterator - [0] Current: ML Crankset.
            // |_Iterator - [1] MoveNext: True.
            // | |_Materialize data row to Product entity.
            // | |_Iterator - [1] Current: HL Crankset.
            // |_Iterator - [2] MoveNext: True.
            // | |_Materialize data row to Product entity.
            // | |_Iterator - [2] Current: Touring-2000 Blue, 60.
            // |_Iterator - [3] MoveNext: False.
            //   |_End.
        }
    }

    internal static partial class Loading
    {
        internal static void ExplicitLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            // SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [p]
            subcategory.Name.WriteLine();

            adventureWorks
                .Entry(subcategory) // Return EntityEntry<ProductSubcategory>.
                .Reference(entity => entity.ProductCategory) // Return ReferenceEntry<ProductSubcategory, ProductCategory>.
                .Load(); // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductCategoryID], [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            subcategory.ProductCategory.Name.WriteLine();

            adventureWorks
                .Entry(subcategory) // Return EntityEntry<ProductSubcategory>.
                .Collection(entity => entity.Products) // Return CollectionEntry<ProductSubcategory, Product>.
                .Load(); // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductID], [e].[ListPrice], [e].[Name], [e].[ProductSubcategoryID]
            // FROM [Production].[Product] AS [e]
            // WHERE [e].[ProductSubcategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            subcategory.Products.WriteLines(product => product.Name);
        }

        internal static void ExplicitLoadingWithQuery(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            // SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [p]
            subcategory.Name.WriteLine();
            string categoryName = adventureWorks
                .Entry(subcategory).Reference(entity => entity.ProductCategory)
                .Query() // Return IQueryable<ProductCategory>.
                .Select(category => category.Name).Single(); // Execute query.
            // exec sp_executesql N'SELECT TOP(2) [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            categoryName.WriteLine();

            IQueryable<string> products = adventureWorks
                .Entry(subcategory).Collection(entity => entity.Products)
                .Query() // Return IQueryable<Product>.
                .Select(product => product.Name); // Execute query.
            // exec sp_executesql N'SELECT [e].[Name]
            // FROM [Production].[Product] AS [e]
            // WHERE [e].[ProductSubcategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            products.WriteLines();
        }

        internal static void EagerLoadingWithInclude(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> subcategoriesWithCategory = adventureWorks.ProductSubcategories
                .Include(subcategory => subcategory.ProductCategory);
            subcategoriesWithCategory.WriteLines(subcategory =>
                $"{subcategory.ProductCategory.Name}: {subcategory.Name}");
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID], [p].[ProductCategoryID], [p].[Name]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // INNER JOIN [Production].[ProductCategory] AS [p] ON [subcategory].[ProductCategoryID] = [p].[ProductCategoryID]

            IQueryable<ProductSubcategory> subcategoriesWithProducts = adventureWorks.ProductSubcategories
                .Include(subcategory => subcategory.Products);
            subcategoriesWithProducts.WriteLines(subcategory => $@"{subcategory.Name}: {string.Join(
                ", ", subcategory.Products.Select(product => product.Name))}");
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // ORDER BY [subcategory].[ProductSubcategoryID]

            // SELECT [p].[ProductID], [p].[ListPrice], [p].[Name], [p].[ProductSubcategoryID], [p].[RowVersion]
            // FROM [Production].[Product] AS [p]
            // WHERE EXISTS (
            //    SELECT 1
            //    FROM [Production].[ProductSubcategory] AS [subcategory]
            //    WHERE [p].[ProductSubcategoryID] = [subcategory].[ProductSubcategoryID])
            // ORDER BY [p].[ProductSubcategoryID]
        }

        internal static void EagerLoadingMultipleLevels(AdventureWorks adventureWorks)
        {
            IQueryable<Product> products = adventureWorks.Products
                .Include(product => product.ProductProductPhotos)
                .ThenInclude(productProductPhoto => productProductPhoto.ProductPhoto);
            products.WriteLines(product => $@"{product.Name}: {string.Join(
                ", ",
                product.ProductProductPhotos.Select(productProductPhoto =>
                    productProductPhoto.ProductPhoto.LargePhotoFileName))}");
            // SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion]
            // FROM [Production].[Product] AS [product]
            // ORDER BY [product].[ProductID]

            // SELECT [p].[ProductID], [p].[ProductPhotoID], [p0].[ProductPhotoID], [p0].[LargePhotoFileName], [p0].[ModifiedDate]
            // FROM [Production].[ProductProductPhoto] AS [p]
            // INNER JOIN [Production].[ProductPhoto] AS [p0] ON [p].[ProductPhotoID] = [p0].[ProductPhotoID]
            // WHERE EXISTS (
            //    SELECT 1
            //    FROM [Production].[Product] AS [product]
            //    WHERE [p].[ProductID] = [product].[ProductID])
            // ORDER BY [p].[ProductID]
        }
    }

    public partial class AdventureWorks
    {
        public AdventureWorks(DbConnection connection = null, bool lazyLoading = true)
            : base(GetDbContextOptions(connection, lazyLoading))
        {
        }

        private static DbContextOptions GetDbContextOptions(
            DbConnection connection = null, bool lazyLoading = true) =>
                new DbContextOptionsBuilder<AdventureWorks>()
                    .UseLazyLoadingProxies(lazyLoading)
                    .UseSqlServer(
                        connection: connection ?? 
                            new SqlConnection(ConnectionStrings.AdventureWorks),
                        sqlServerOptionsAction: options => options.EnableRetryOnFailure(
                            maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null))
                    .Options;
    }

    internal static partial class Loading
    {
        internal static void LazyLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
            // SELECT TOP(1) [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [p]
            subcategory.Name.WriteLine();

            ProductCategory category = subcategory.ProductCategory; // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductCategoryID], [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            category.Name.WriteLine();

            ICollection<Product> products = subcategory.Products; // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductID], [e].[ListPrice], [e].[Name], [e].[ProductSubcategoryID], [e].[RowVersion]
            // FROM [Production].[Product] AS [e]
            // WHERE [e].[ProductSubcategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1
            products.WriteLines(product => product.Name);
        }

        internal static void MultipleLazyLoading(AdventureWorks adventureWorks)
        {
            ProductSubcategory[] subcategories = adventureWorks.ProductSubcategories.ToArray(); // Execute query.
            // SELECT [p].[ProductSubcategoryID], [p].[Name], [p].[ProductCategoryID]
            // FROM [Production].[ProductSubcategory] AS [p]

            subcategories.WriteLines(subcategory =>
                $"{subcategory.Name} ({subcategory.ProductCategory.Name})"); // Execute query.
            // exec sp_executesql N'SELECT [e].[ProductCategoryID], [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=1

            // exec sp_executesql N'SELECT [e].[ProductCategoryID], [e].[Name]
            // FROM [Production].[ProductCategory] AS [e]
            // WHERE [e].[ProductCategoryID] = @__get_Item_0',N'@__get_Item_0 int',@__get_Item_0=2

            // ...
        }

        internal static void EagerLoadingWithSelect(AdventureWorks adventureWorks)
        {
            var subcategories = adventureWorks.ProductSubcategories.Select(subcategory => new
            {
                Name = subcategory.Name,
                CategoryName = subcategory.ProductCategory.Name,
                ProductCount = subcategory.Products.Count
            });
            subcategories.WriteLines(subcategory =>
                $"{subcategory.CategoryName}/{subcategory.Name}: {subcategory.ProductCount}");
            // SELECT [subcategory].[Name] AS [Name0], [subcategory.ProductCategory].[Name] AS [CategoryName], (
            //    SELECT COUNT(*)
            //    FROM [Production].[Product] AS [p]
            //    WHERE [subcategory].[ProductSubcategoryID] = [p].[ProductSubcategoryID]
            // ) AS [ProductCount]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // INNER JOIN [Production].[ProductCategory] AS [subcategory.ProductCategory] ON [subcategory].[ProductCategoryID] = [subcategory.ProductCategory].[ProductCategoryID]
        }

        internal static void ConditionalEagerLoadingWithInclude(AdventureWorks adventureWorks)
        {
            IQueryable<ProductSubcategory> subcategories = adventureWorks.ProductSubcategories
                .Include(subcategory => subcategory.Products.Where(product => product.ListPrice > 0));
            subcategories.WriteLines(subcategory =>
                $@"{subcategory.Name}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.Name))}");
            // System.InvalidOperationException: The Include property lambda expression 'subcategory => {from Product product in subcategory.Products where ([product].ListPrice > 0) select [product]}' is invalid. The expression should represent a property access: 't => t.MyProperty'. To target navigations declared on derived types, specify an explicitly typed lambda parameter of the target type, E.g. '(Derived d) => d.MyProperty'. For more information on including related data, see http://go.microsoft.com/fwlink/?LinkID=746393.
        }

        internal static void ConditionalEagerLoadingWithJoin(AdventureWorks adventureWorks)
        {
            var subcategories = adventureWorks.ProductSubcategories.GroupJoin(
                inner: adventureWorks.Products.Where(product => product.ListPrice > 0),
                outerKeySelector: subcategory => subcategory.ProductSubcategoryID,
                innerKeySelector: product => product.ProductSubcategoryID,
                resultSelector: (subcategory, products) => new
                {
                    Subcategory = subcategory,
                    Products = products
                });
            subcategories.WriteLines(subcategory =>
                $@"{subcategory.Subcategory.Name}: {string.Join(
                    ", ", subcategory.Products.Select(product => product.Name))}");
            // SELECT [subcategory].[ProductSubcategoryID], [subcategory].[Name], [subcategory].[ProductCategoryID], [t].[ProductID], [t].[ListPrice], [t].[Name], [t].[ProductSubcategoryID], [t].[RowVersion]
            // FROM [Production].[ProductSubcategory] AS [subcategory]
            // LEFT JOIN (
            //    SELECT [product].[ProductID], [product].[ListPrice], [product].[Name], [product].[ProductSubcategoryID], [product].[RowVersion]
            //    FROM [Production].[Product] AS [product]
            //    WHERE [product].[ListPrice] > 0.0
            // ) AS [t] ON [subcategory].[ProductSubcategoryID] = [t].[ProductSubcategoryID]
            // ORDER BY [subcategory].[ProductSubcategoryID]
        }

        internal static void DisableLazyLoading()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks(lazyLoading: false))
            {
                ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First(); // Execute query.
                subcategory.Name.WriteLine();
                ProductCategory category = subcategory.ProductCategory; // No query.
                (category == null).WriteLine(); // True

                ICollection<Product> products = subcategory.Products; // No query.
                (products == null).WriteLine(); // True
            }
        }

        internal static class DataAccess
        {
            internal static IQueryable<Product> QueryCategoryProducts(string category)
            {
                using (AdventureWorks adventureWorks = new AdventureWorks())
                {
                    return adventureWorks.Products.Where(
                        product => product.ProductSubcategory.ProductCategory.Name == category);
                }
            }
        }

        internal static class UI
        {
            internal static void RenderCategoryProducts(string category) => DataAccess
                .QueryCategoryProducts(category)
                .Select(product => product.Name)
                .WriteLines();
            // InvalidOperationException: The operation cannot be completed because the DbContext has been disposed.
        }
    }
}

#if DEMO
namespace Tutorial.LinqToEntities
{
    public partial class AdventureWorks
    {
        public AdventureWorks()
            : base(ConnectionStrings.AdventureWorks)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}

namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;

    public class DbContext
    {
        public DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class DbEntityEntry<TEntity> where TEntity : class
    {
        public DbReferenceEntry<TEntity, TProperty> Reference<TProperty>(
            Expression<Func<TEntity, TProperty>> navigationProperty) where TProperty : class;

        public DbCollectionEntry<TEntity, TElement> Collection<TElement>(
            Expression<Func<TEntity, ICollection<TElement>>> navigationProperty) where TElement : class;

        // Other members.
    }
}
#endif
