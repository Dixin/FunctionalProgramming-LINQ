namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage;

    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

    using IsolationLevel = System.Data.IsolationLevel;
    using RefreshConflict = Concurrency.RefreshConflict;

    internal static partial class Performance
    {
        internal static void AddRange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.ProductCategories.Load(); // Warm up.
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<ProductCategory> categories = Enumerable
                    .Range(0, 100).Select(index => new ProductCategory() { Name = index.ToString() });
                DbSet<ProductCategory> repository = adventureWorks.ProductCategories;
                foreach (ProductCategory category in categories)
                {
                    repository.Add(category);
                }
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 1682
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                IEnumerable<ProductCategory> categories = Enumerable
                    .Range(0, 100).Select(index => new ProductCategory() { Name = index.ToString() });
                adventureWorks.ProductCategories.AddRange(categories);
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 2
            }
        }

        internal static void RemoveRange()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Products.Load(); // Warm up.
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Product[] products = adventureWorks.Products.ToArray();
                DbSet<Product> repository = adventureWorks.Products;
                foreach (Product product in products)
                {
                    repository.Remove(product);
                }
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 1682
            }
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Product[] products = adventureWorks.Products.ToArray();
                adventureWorks.Products.RemoveRange(products);
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine(); // 2
            }
        }

        internal static void CachedEntity(AdventureWorks adventureWorks)
        {
            ProductCategory categoryCopy1 = adventureWorks.ProductCategories
                .Single(entity => entity.ProductCategoryID == 1);
            categoryCopy1.Name = "Cache";

            ProductCategory categoryCopy2 = adventureWorks.ProductCategories
                .Single(entity => entity.Name == "Bikes");
            categoryCopy2.Name.WriteLine(); // Cache
            object.ReferenceEquals(categoryCopy1, categoryCopy2).WriteLine(); // True

            ProductCategory categoryCopy3 = adventureWorks.ProductCategories
                .FromSql(
                    @"SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                    FROM [Production].[ProductCategory]
                    ORDER BY [ProductCategory].[ProductCategoryID]")
                .Single();
            object.ReferenceEquals(categoryCopy1, categoryCopy3).WriteLine(); // True
        }

        internal static void UncachedEntity(AdventureWorks adventureWorks)
        {
            ProductCategory categoryCopy1 = adventureWorks.ProductCategories
                .Single(entity => entity.ProductCategoryID == 1);
            categoryCopy1.Name = "Cache";

            ProductCategory categoryCopy2 = adventureWorks.ProductCategories
                .AsNoTracking().Single(entity => entity.Name == "Bikes");
            categoryCopy2.Name.WriteLine(); // Bikes
            object.ReferenceEquals(categoryCopy1, categoryCopy2).WriteLine(); // False

            ProductCategory categoryCopy3 = adventureWorks.ProductCategories
                .FromSql(
                    @"SELECT TOP (1) [ProductCategory].[ProductCategoryID], [ProductCategory].[Name]
                    FROM [Production].[ProductCategory]
                    ORDER BY [ProductCategory].[ProductCategoryID]")
                .AsNoTracking()
                .Single();
            object.ReferenceEquals(categoryCopy1, categoryCopy3).WriteLine(); // False
        }

        internal static void Find(AdventureWorks adventureWorks)
        {
            Product[] products = adventureWorks.Products
#pragma warning disable CA1307 // Specify StringComparison
                .Where(entity => entity.Name.StartsWith("Road")).ToArray(); // Execute query.
#pragma warning restore CA1307 // Specify StringComparison
            Product product = adventureWorks.Products.Find(999); // No database query.
            object.ReferenceEquals(products.Last(), product).WriteLine(); // True
        }

        internal static void TranslationCache(AdventureWorks adventureWorks)
        {
            int minLength = 1;
            IQueryable<Product> query = adventureWorks.Products
                .Where(product => product.Name.Length >= minLength)
                .Include(product => product.ProductSubcategory);
            query.Load();
        }

        internal static void UnreusedTranslationCache(AdventureWorks adventureWorks)
        {
            IQueryable<Product> queryWithConstant1 = adventureWorks.Products
                .Where(product => product.Name.Length >= 1);
            queryWithConstant1.Load();

            IQueryable<Product> queryWithConstant2 = adventureWorks.Products
                .Where(product => product.Name.Length >= 10);
            queryWithConstant2.Load();
        }

        internal static void ReusedTranslationCache(AdventureWorks adventureWorks)
        {
            int minLength = 1;
            IQueryable<Product> queryWithClosure1 = adventureWorks.Products
                .Where(product => product.Name.Length >= minLength);
            queryWithClosure1.Load();

            minLength = 10;
            IQueryable<Product> queryWithClosure2 = adventureWorks.Products
                .Where(product => product.Name.Length >= minLength);
            queryWithClosure2.Load();
        }

        [CompilerGenerated]
        private sealed class DisplayClass { public int MinLength; }

        internal static void CompiledReusedTranslationCache(AdventureWorks adventureWorks)
        {
            DisplayClass displayClass = new DisplayClass() { MinLength = 1 };
            IQueryable<Product> queryWithClosure1 = adventureWorks.Products
                .Where(product => product.Name.Length >= displayClass.MinLength);
            queryWithClosure1.Load();

            displayClass.MinLength = 10;
            IQueryable<Product> queryWithClosure2 = adventureWorks.Products
                .Where(product => product.Name.Length >= displayClass.MinLength);
            queryWithClosure2.Load();
        }

        internal static void Translation()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                adventureWorks.Products.Load(); // Warm up.
            }

            Func<string, Expression<Func<Product, bool>>> getPredicateWithConstant = startWith =>
                {
                    ParameterExpression productParameterExpression = Expression.Parameter(typeof(Product), "product");
                    Func<string, bool> startsWithMethod = string.Empty.StartsWith;
                    Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                        Expression.Call(
                            instance: Expression.Property(productParameterExpression, nameof(Product.Name)),
                            method: startsWithMethod.Method,
                            arguments: Expression.Constant(startWith, typeof(string))),
                        productParameterExpression);
                    return predicateExpression;
                };

            Func<string, Expression<Func<Product, bool>>> getPredicateWithVariable =
#pragma warning disable CA1307 // Specify StringComparison
                startWith => product => product.Name.StartsWith(startWith);
#pragma warning restore CA1307 // Specify StringComparison

            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Enumerable.Range(0, 1_000).ForEach(value =>
                {
                    IQueryable<Product> query = adventureWorks.Products
                        .Where(getPredicateWithConstant(value.ToString()));
                    query.Load();
                });
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine();

                stopwatch.Restart();
                Enumerable.Range(0, 1_000).ForEach(value =>
                {
                    IQueryable<Product> query = adventureWorks.Products
                        .Where(getPredicateWithVariable(value.ToString()));
                    query.Load();
                });
                stopwatch.Stop();
                stopwatch.ElapsedMilliseconds.WriteLine();
            }
        }

        internal static void UnreusedSkipTakeTranslationCache(AdventureWorks adventureWorks)
        {
            int skip = 1;
            int take = 1;
            IQueryable<Product> skipTakeWithVariable1 = adventureWorks.Products
                .OrderBy(product => product.ProductID).Skip(skip).Take(take);
            skipTakeWithVariable1.Load();

            skip = 10;
            take = 10;
            IQueryable<Product> skipTakeWithVariable2 = adventureWorks.Products
                .OrderBy(product => product.ProductID).Skip(skip).Take(take);
            skipTakeWithVariable2.Load();
        }

        internal static async Task Async(AdventureWorks adventureWorks)
        {
            IQueryable<ProductCategory> categories = adventureWorks.ProductCategories;
            await categories.ForEachAsync( // Async version of foreach/ForEach.
                category => category.Name.WriteLine());

            ProductSubcategory subcategory = await adventureWorks.ProductSubcategories
                .FirstAsync(entity => entity.Name.Contains("Bike")); // Async version of First.
            subcategory.Name.WriteLine();

            Product[] products = await adventureWorks.Products
                .Where(product => product.ListPrice <= 10)
                .ToArrayAsync(); // Async version of ToArray.

            adventureWorks.Products.RemoveRange(products);
            (await adventureWorks.SaveChangesAsync()).WriteLine(); // Async version of SaveChanges.
        }

        internal static async Task<EntityEntry> RefreshAsync(this EntityEntry tracking, RefreshConflict refreshMode)
        {
            switch (refreshMode)
            {
                case RefreshConflict.StoreWins:
                {
                    await tracking.ReloadAsync();
                    break;
                }
                case RefreshConflict.ClientWins:
                {
                    PropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        tracking.State = EntityState.Detached;
                    }
                    else
                    {
                        tracking.OriginalValues.SetValues(databaseValues);
                    }
                    break;
                }
                case RefreshConflict.MergeClientAndStore:
                {
                    PropertyValues databaseValues = await tracking.GetDatabaseValuesAsync();
                    if (databaseValues == null)
                    {
                        tracking.State = EntityState.Detached;
                    }
                    else
                    {
                        PropertyValues originalValues = tracking.OriginalValues.Clone();
                        tracking.OriginalValues.SetValues(databaseValues);
                        databaseValues.Properties
                            .Where(property => !object.Equals(originalValues[property.Name], databaseValues[property.Name]))
                            .ForEach(property => tracking.Property(property.Name).IsModified = false);
                    }
                    break;
                }
            }
            return tracking;
        }

        public static async Task<int> SaveChangesAsync(
            this DbContext context, Func<IEnumerable<EntityEntry>, Task> resolveConflictsAsync, int retryCount = 3)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            for (int retry = 1; retry < retryCount; retry++)
            {
                try
                {
                    return await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException exception) when (retry < retryCount)
                {
                    await resolveConflictsAsync(exception.Entries);
                }
            }
            return await context.SaveChangesAsync();
        }

        public static async Task<int> SaveChangesAsync(
            this DbContext context, Func<IEnumerable<EntityEntry>, Task> resolveConflictsAsync, RetryStrategy retryStrategy)
        {
            RetryPolicy retryPolicy = new RetryPolicy(
                new Concurrency.TransientDetection<DbUpdateConcurrencyException>(), retryStrategy);
            retryPolicy.Retrying += (sender, e) =>
                resolveConflictsAsync(((DbUpdateConcurrencyException)e.LastException).Entries).Wait();
            return await retryPolicy.ExecuteAsync(async () => await context.SaveChangesAsync());
        }

        internal static async Task<int> SaveChangesAsync(
            this DbContext context, RefreshConflict refreshMode, int retryCount = 3)
        {
            if (retryCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retryCount));
            }

            return await context.SaveChangesAsync(
                async conflicts => await Task.WhenAll(conflicts.Select(async tracking =>
                    await tracking.RefreshAsync(refreshMode))),
                retryCount);
        }

        internal static async Task<int> SaveChangesAsync(
            this DbContext context, RefreshConflict refreshMode, RetryStrategy retryStrategy) =>
                await context.SaveChangesAsync(
                    async conflicts => await Task.WhenAll(conflicts.Select(async tracking =>
                        await tracking.RefreshAsync(refreshMode))),
                    retryStrategy);

        internal static async Task SaveChangesAsync()
        {
            using (AdventureWorks adventureWorks1 = new AdventureWorks())
            using (AdventureWorks adventureWorks2 = new AdventureWorks())
            {
                int id = 950;
                Product productCopy1 = await adventureWorks1.Products.FindAsync(id);
                Product productCopy2 = await adventureWorks2.Products.FindAsync(id);

                productCopy1.Name = nameof(productCopy1);
                productCopy1.ListPrice = 100;
                (await adventureWorks1.SaveChangesAsync()).WriteLine(); // 1

                productCopy2.Name = nameof(productCopy2);
                productCopy2.ProductSubcategoryID = 1;
                (await adventureWorks2.SaveChangesAsync(RefreshConflict.MergeClientAndStore)).WriteLine(); // 1
            }
        }

        internal static async Task DbContextTransactionAsync(AdventureWorks adventureWorks)
        {
            await adventureWorks.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                using (IDbContextTransaction transaction = await adventureWorks.Database.BeginTransactionAsync(
                    IsolationLevel.ReadUncommitted))
                {
                    try
                    {
                        adventureWorks.CurrentIsolationLevel().WriteLine(); // ReadUncommitted

                        ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                        await adventureWorks.ProductCategories.AddAsync(category);
                        (await adventureWorks.SaveChangesAsync()).WriteLine(); // 1

                        await adventureWorks.Database.ExecuteSqlCommandAsync(
                            sql: "DELETE FROM [Production].[ProductCategory] WHERE [Name] = {0}",
                            parameters: nameof(ProductCategory)).WriteLine(); // 1
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        internal static async Task DbTransactionAsync()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                await connection.OpenAsync();
                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        using (AdventureWorks adventureWorks = new AdventureWorks(connection))
                        {
                            await adventureWorks.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                            {
                                adventureWorks.Database.UseTransaction(transaction);
                                adventureWorks.CurrentIsolationLevel().WriteLine(); // Serializable

                                ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                                await adventureWorks.ProductCategories.AddAsync(category);
                                (await adventureWorks.SaveChangesAsync()).WriteLine(); // 1.
                            });
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @p0";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@p0";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);
                            command.Transaction = transaction;
                            (await command.ExecuteNonQueryAsync()).WriteLine(); // 1
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
