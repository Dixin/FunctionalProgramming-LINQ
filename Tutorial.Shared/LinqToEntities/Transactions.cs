namespace Tutorial.LinqToEntities
{
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    using IsolationLevel = System.Data.IsolationLevel;

    internal static partial class Transactions
    {
        internal static void ExecutionStrategy(AdventureWorks adventureWorks)
        {
            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
            {
                // Single retry operation, which can have custom transactions.
            });
        }

        internal static void Default(AdventureWorks adventureWorks)
        {
            ProductCategory category = adventureWorks.ProductCategories.First();
            category.Name = "Update"; // Valid value.g
            ProductSubcategory subcategory = adventureWorks.ProductSubcategories.First();
            subcategory.ProductCategoryID = -1; // Invalid value.
            try
            {
                adventureWorks.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                exception.WriteLine();
                // System.Data.Entity.Infrastructure.DbUpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> System.Data.Entity.Core.UpdateException: An error occurred while updating the entries. See the inner exception for details.
                // ---> System.Data.SqlClient.SqlException: The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_ProductSubcategory_ProductCategory_ProductCategoryID". The conflict occurred in database "D:\ONEDRIVE\WORKS\DRAFTS\CODESNIPPETS\DATA\ADVENTUREWORKS_DATA.MDF", table "Production.ProductCategory", column 'ProductCategoryID'. The statement has been terminated.
                adventureWorks.Entry(category).Reload();
                category.Name.WriteLine(); // Accessories
                adventureWorks.Entry(subcategory).Reload();
                subcategory.ProductCategoryID.WriteLine(); // 1
            }
        }

        internal static IsolationLevel CurrentIsolationLevel(this DbConnection connection,
            DbTransaction transaction = null)
        {
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT transaction_isolation_level FROM sys.dm_exec_sessions WHERE session_id = @@SPID";
                command.Transaction = transaction;
                switch ((short)command.ExecuteScalar())
                {
                    case 0: return IsolationLevel.Unspecified;
                    case 1: return IsolationLevel.ReadUncommitted;
                    case 2: return IsolationLevel.ReadCommitted;
                    case 3: return IsolationLevel.RepeatableRead;
                    case 4: return IsolationLevel.Serializable;
                    case 5: return IsolationLevel.Snapshot;
                    default: throw new InvalidOperationException();
                }
            }
        }

        internal static IsolationLevel CurrentIsolationLevel(this DbContext dbContext) =>
            dbContext.Database.GetDbConnection().CurrentIsolationLevel(
                dbContext.Database.CurrentTransaction?.GetDbTransaction());

        internal static void DbContextTransaction(AdventureWorks adventureWorks)
        {
            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (IDbContextTransaction transaction = adventureWorks.Database
                        .BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        try
                        {
                            ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                            adventureWorks.ProductCategories.Add(category);
                            adventureWorks.SaveChanges().WriteLine(); // 1

                            adventureWorks.Database
                                .ExecuteSqlCommand($@"DELETE FROM [Production].[ProductCategory] WHERE [Name] = {nameof(ProductCategory)}")
                                .WriteLine(); // 1
                            adventureWorks.CurrentIsolationLevel().WriteLine(); // ReadUncommitted
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
    }

    internal static partial class Transactions
    {
        internal static void DbTransaction()
        {
            using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
            {
                connection.Open();
                using (DbTransaction transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        using (AdventureWorks adventureWorks = new AdventureWorks(connection))
                        {
                            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
                                {
                                    adventureWorks.Database.UseTransaction(transaction);
                                    adventureWorks.CurrentIsolationLevel().WriteLine(); // RepeatableRead

                                    ProductCategory category = new ProductCategory() { Name = nameof(ProductCategory) };
                                    adventureWorks.ProductCategories.Add(category);
                                    adventureWorks.SaveChanges().WriteLine(); // 1.
                                });
                        }

                        using (DbCommand command = connection.CreateCommand())
                        {
                            command.CommandText = "DELETE FROM [Production].[ProductCategory] WHERE [Name] = @Name";
                            DbParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@Name";
                            parameter.Value = nameof(ProductCategory);
                            command.Parameters.Add(parameter);
                            command.Transaction = transaction;
                            command.ExecuteNonQuery().WriteLine(); // 1
                            connection.CurrentIsolationLevel(transaction).WriteLine(); // RepeatableRead
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

namespace Tutorial.LinqToEntities
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Transactions;
    using Microsoft.EntityFrameworkCore;

    internal static partial class Transactions
    {
        internal static void TransactionScope(AdventureWorks adventureWorks)
        {
            adventureWorks.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (TransactionScope scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions() { IsolationLevel = IsolationLevel.Serializable }))
                {
                    using (DbConnection connection = new SqlConnection(ConnectionStrings.AdventureWorks))
                    using (DbCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "INSERT INTO [Production].[ProductCategory] ([Name]) VALUES(@Name); ";
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = "@Name";
                        parameter.Value = nameof(ProductCategory);
                        command.Parameters.Add(parameter);

                        connection.Open();
                        command.ExecuteNonQuery().WriteLine(); // 1
                        connection.CurrentIsolationLevel().WriteLine(); // Serializable
                    }

                    using (AdventureWorks adventureWorks1 = new AdventureWorks())
                    {
                        ProductCategory category = adventureWorks1.ProductCategories
                            .Single(entity => entity.Name == nameof(ProductCategory));
                        adventureWorks1.ProductCategories.Remove(category);
                        adventureWorks1.SaveChanges().WriteLine(); // 1
                        adventureWorks1.CurrentIsolationLevel().WriteLine(); // Serializable
                    }

                    scope.Complete();
                }
            });
        }
    }
}
