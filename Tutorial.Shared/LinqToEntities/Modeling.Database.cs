namespace Tutorial.LinqToEntities
{
    using Microsoft.EntityFrameworkCore;

    public partial class AdventureWorks : DbContext { }

#if DEMO
    public partial class AdventureWorks
    {
        public AdventureWorks(DbConnection connection = null)
            : base(GetDbContextOptions(connection))
        {
        }
    
        private static DbContextOptions GetDbContextOptions(
            DbConnection connection = null) =>
                new DbContextOptionsBuilder<AdventureWorks>()
                    .UseSqlServer(
                        connection: connection ?? 
                            new SqlConnection(ConnectionStrings.AdventureWorks),
                        sqlServerOptionsAction: options => options.EnableRetryOnFailure(
                            maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), 
                            errorNumbersToAdd: null))
                    .Options;
    }
#endif

    public partial class AdventureWorks
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            MapCompositePrimaryKey(modelBuilder);
            MapManyToMany(modelBuilder);
            MapDiscriminator(modelBuilder);
        }
    }
}

#if DEMO
namespace Microsoft.EntityFrameworkCore
{
    using System;

    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    public class DbContext : IDisposable, IInfrastructure<IServiceProvider>
    {
        public DbContext(DbContextOptions options);

        public virtual ChangeTracker ChangeTracker { get; }

        public virtual DatabaseFacade Database { get; }

        public virtual void Dispose();

        public virtual int SaveChanges();

        public virtual DbSet<TEntity> Set<TEntity>() where TEntity : class;

        protected internal virtual void OnModelCreating(ModelBuilder modelBuilder);

        // Other members.
    }
}
#endif
