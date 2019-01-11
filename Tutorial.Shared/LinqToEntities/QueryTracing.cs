namespace Tutorial.LinqToEntities
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class TraceLogger : ILogger
    {
        private readonly string categoryName;

        public TraceLogger(string categoryName) => this.categoryName = categoryName;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Trace.WriteLine($"{DateTime.Now.ToString("o")} {logLevel} {eventId.Id} {this.categoryName}");
            Trace.WriteLine(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new TraceLogger(categoryName);

        public void Dispose() { }
    }

    public partial class AdventureWorks
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            LoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new TraceLoggerProvider());
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }
    }

    internal static partial class Tracing
    {
        internal static void TraceLogger()
        {
            using (AdventureWorks adventureWorks = new AdventureWorks())
            {
                IQueryable<ProductCategory> source = adventureWorks.ProductCategories; // Define query.
                source.WriteLines(category => category.Name); // Execute query.
            }
            // 2017-01-11T22:15:43.4625876-08:00 Debug 2 Microsoft.EntityFrameworkCore.Query.Internal.SqlServerQueryCompilationContextFactory
            // Compiling query model:
            // 'from ProductCategory <generated>_0 in DbSet<ProductCategory>
            // select <generated>_0'

            // 2017-01-11T22:15:43.4932882-08:00 Debug 3 Microsoft.EntityFrameworkCore.Query.Internal.SqlServerQueryCompilationContextFactory
            // Optimized query model:
            // 'from ProductCategory <generated>_0 in DbSet<ProductCategory>
            // select <generated>_0'

            // 2017-01-11T22:15:43.6179834-08:00 Debug 5 Microsoft.EntityFrameworkCore.Query.Internal.SqlServerQueryCompilationContextFactory
            // TRACKED: True
            // (QueryContext queryContext) => IEnumerable<ProductCategory> _ShapedQuery(
            //    queryContext: queryContext,
            //    shaperCommandContext: SelectExpression:
            //        SELECT [p].[ProductCategoryID], [p].[Name]
            //        FROM [Production].[ProductCategory] AS [p]
            //    ,
            //    shaper: UnbufferedEntityShaper<ProductCategory>
            // )

            // 2017-01-11T22:15:43.7272876-08:00 Debug 3 Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerConnection
            // Opening connection to database 'AdventureWorks' on server 'tcp:tutorialsql.database.windows.net,1433'.

            // 2017-01-11T22:15:44.1024201-08:00 Information 1 Microsoft.EntityFrameworkCore.Storage.IRelationalCommandBuilderFactory
            // Executed DbCommand (66ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
            // SELECT [p].[ProductCategoryID], [p].[Name]
            // FROM [Production].[ProductCategory] AS [p]

            // 2017-01-11T22:15:44.1505353-08:00 Debug 4 Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerConnection
            // Closing connection to database 'AdventureWorks' on server 'tcp:tutorialsql.database.windows.net,1433'.
        }
    }
}

#if DEMO
namespace System.Data.Entity
{
    using System.Data.Entity.Infrastructure;

    public class DbContext : IDisposable, IObjectContextAdapter
    {
        public Database Database { get; }

        // Other members.
    }

    public class Database
    {
        public Action<string> Log { get; set; }

        // Other members.
    }
}

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Common;

    public interface IDbCommandInterceptor : IDbInterceptor // IDbInterceptor is an empty interface.
    {
        void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

        void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext);

        void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

        void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext);

        void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);

        void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext);
    }
}
#endif
