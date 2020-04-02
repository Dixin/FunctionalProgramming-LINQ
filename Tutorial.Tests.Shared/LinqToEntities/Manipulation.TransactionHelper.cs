namespace Tutorial.Tests.LinqToEntities
{
    using System;
    using System.Data.Common;

    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Tutorial.LinqToEntities;

    internal static class TransactionHelper
    {
        internal static string DefaultConnection { get; set; } = ConnectionStrings.AdventureWorks;

        internal static void Rollback(Action<AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        using (DbTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                adventureWorks1.Database.UseTransaction(transaction);
                                action(adventureWorks1);
                            }
                            finally
                            {
                                transaction.Rollback();
                            }
                        }
                    });
                }
            }
        }

        internal static void Rollback(Action<AdventureWorks, AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks2 = new AdventureWorks(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        adventureWorks2.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            using (DbTransaction transaction = connection.BeginTransaction())
                            {
                                try
                                {
                                    adventureWorks1.Database.UseTransaction(transaction);
                                    adventureWorks2.Database.UseTransaction(transaction);
                                    action(adventureWorks1, adventureWorks2);
                                }
                                finally
                                {
                                    transaction.Rollback();
                                }
                            }
                        });
                    });
                }
            }
        }

        internal static void Rollback(Action<AdventureWorks, AdventureWorks, AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks2 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks3 = new AdventureWorks(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        adventureWorks2.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            adventureWorks3.Database.CreateExecutionStrategy().Execute(() =>
                            {
                                using (DbTransaction transaction = connection.BeginTransaction())
                                {
                                    try
                                    {
                                        adventureWorks1.Database.UseTransaction(transaction);
                                        adventureWorks2.Database.UseTransaction(transaction);
                                        adventureWorks3.Database.UseTransaction(transaction);
                                        action(adventureWorks1, adventureWorks2, adventureWorks3);
                                    }
                                    finally
                                    {
                                        transaction.Rollback();
                                    }
                                }
                            });
                        });
                    });
                }
            }
        }

        internal static void Rollback(Action<AdventureWorks, AdventureWorks, AdventureWorks, AdventureWorks> action, string connectionString = null)
        {
            using (DbConnection connection = new SqlConnection(connectionString ?? DefaultConnection))
            {
                connection.Open();
                using (AdventureWorks adventureWorks1 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks2 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks3 = new AdventureWorks(connection))
                using (AdventureWorks adventureWorks4 = new AdventureWorks(connection))
                {
                    adventureWorks1.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        adventureWorks2.Database.CreateExecutionStrategy().Execute(() =>
                        {
                            adventureWorks3.Database.CreateExecutionStrategy().Execute(() =>
                            {
                                adventureWorks4.Database.CreateExecutionStrategy().Execute(() =>
                                {
                                    using (DbTransaction transaction = connection.BeginTransaction())
                                    {
                                        try
                                        {
                                            adventureWorks1.Database.UseTransaction(transaction);
                                            adventureWorks2.Database.UseTransaction(transaction);
                                            adventureWorks3.Database.UseTransaction(transaction);
                                            adventureWorks4.Database.UseTransaction(transaction);
                                            action(adventureWorks1, adventureWorks2, adventureWorks3, adventureWorks4);
                                        }
                                        finally
                                        {
                                            transaction.Rollback();
                                        }
                                    }
                                });
                            });
                        });
                    });
                }
            }
        }
    }
}
