namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
    using Microsoft.EntityFrameworkCore.Storage;

    internal static partial class Translation
    {
        internal static void WhereAndSelect(AdventureWorks adventureWorks)
        {
            // IQueryable<string> products = adventureWorks.Products
            //    .Where(product => product.Name.Length > 10)
            //    .Select(product => product.Name);
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<Product> whereQueryable = sourceQueryable.Where(product => product.Name.Length > 10);
            IQueryable<string> selectQueryable = whereQueryable.Select(product => product.Name); // Define query.
            foreach (string result in selectQueryable) // Execute query.
            {
                result.WriteLine();
            }
        }

        internal static void WhereAndSelectLinqExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products; // DbSet<Product>.
            ConstantExpression sourceConstantExpression = (ConstantExpression)sourceQueryable.Expression;
            IQueryProvider sourceQueryProvider = sourceQueryable.Provider; // EntityQueryProvider/DbQueryProvider.

            // Expression<Func<Product, bool>> predicateExpression = product => product.Name.Length > 10;
            ParameterExpression productParameterExpression = Expression.Parameter(typeof(Product), "product");
            Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                body: Expression.GreaterThan(
                    left: Expression.Property(
                        expression: Expression.Property(
                            expression: productParameterExpression, propertyName: nameof(Product.Name)),
                        propertyName: nameof(string.Length)),
                    right: Expression.Constant(10)),
                parameters: productParameterExpression);

            // IQueryable<Product> whereQueryable = sourceQueryable.Where(predicateExpression);
            Func<IQueryable<Product>, Expression<Func<Product, bool>>, IQueryable<Product>> whereMethod =
                Queryable.Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: whereMethod.Method,
                arg0: sourceConstantExpression,
                arg1: Expression.Quote(predicateExpression));
            IQueryable<Product> whereQueryable = sourceQueryProvider
                .CreateQuery<Product>(whereCallExpression); // EntityQueryable<Product>/DbQuery<Product>.
            IQueryProvider whereQueryProvider = whereQueryable.Provider; // EntityQueryProvider/DbQueryProvider.

            // Expression<Func<Product, string>> selectorExpression = product => product.Name;
            Expression<Func<Product, string>> selectorExpression = Expression.Lambda<Func<Product, string>>(
                body: Expression.Property(productParameterExpression, nameof(Product.Name)),
                parameters: productParameterExpression);

            // IQueryable<string> selectQueryable = whereQueryable.Select(selectorExpression);
            Func<IQueryable<Product>, Expression<Func<Product, string>>, IQueryable<string>> selectMethod =
                Queryable.Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: selectMethod.Method,
                arg0: whereCallExpression,
                arg1: Expression.Quote(selectorExpression));
            IQueryable<string> selectQueryable = whereQueryProvider
                .CreateQuery<string>(selectCallExpression); // EntityQueryable<Product>/DbQuery<Product>.
            
            using (IEnumerator<string> iterator = selectQueryable.GetEnumerator()) // Execute query.
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.WriteLine();
                }
            }
        }

        internal static SelectExpression WhereAndSelectDatabaseExpressions(AdventureWorks adventureWorks)
        {
            IQueryableMethodTranslatingExpressionVisitorFactory expressionVisitorFactory = adventureWorks
                .GetService<IQueryableMethodTranslatingExpressionVisitorFactory>();
            ISqlExpressionFactory expressionFactory = adventureWorks.GetService<ISqlExpressionFactory>();
            IEntityType entityType = adventureWorks.Model.FindEntityType(typeof(Product));
            SelectExpression databaseExpression = expressionFactory.Select(entityType);
            EntityProjectionExpression projectionExpression = (EntityProjectionExpression)databaseExpression.GetMappedProjection(new ProjectionMember());
            ColumnExpression columnExpression = projectionExpression.BindProperty(entityType.FindProperty(nameof(Product.Name)));
            databaseExpression.ApplyPredicate(expressionFactory.MakeBinary(
                ExpressionType.GreaterThan,
                expressionFactory.Convert(
                    expressionFactory.Function("LEN", new SqlExpression[] { columnExpression }, typeof(long)),
                    typeof(int)),
                new SqlConstantExpression(Expression.Constant(10), null),
                null));
            databaseExpression.AddToProjection(columnExpression);
            return databaseExpression.WriteLine();
        }

        internal static void WhereAndSelectQuery(AdventureWorks adventureWorks)
        {
            IQueryable<string> products = adventureWorks.Products
               .Where(product => product.Name.Length > 10)
               .Select(product => product.Name);
            // Equivalent to:
            // IQueryable<string> products =
            //    from product in adventureWorks.Products
            //    where product.Name.Length > 10
            //    select product.Name;
        }

        internal static void CompileWhereAndSelectExpressions(AdventureWorks adventureWorks)
        {
            Expression linqExpression = adventureWorks.Products
                .Where(product => product.Name.Length > 10)
                .Select(product => product.Name).Expression;
            (SelectExpression DatabaseExpression, IReadOnlyDictionary<string, object> Parameters) result =
                adventureWorks.Compile(linqExpression);
            result.DatabaseExpression.WriteLine();
            result.Parameters.WriteLines(parameter => $"{parameter.Key}: {parameter.Value}");
        }

        internal static void SelectAndFirst(AdventureWorks adventureWorks)
        {
            // string first = adventureWorks.Products.Select(product => product.Name).First();
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            string first = selectQueryable.First().WriteLine(); // Execute query.
        }

        internal static void SelectAndFirstLinqExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products;

            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            MethodCallExpression selectCallExpression = (MethodCallExpression)selectQueryable.Expression;
            IQueryProvider selectQueryProvider = selectQueryable.Provider; // DbQueryProvider.

            // string first = selectQueryable.First();
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: firstMethod.Method, arg0: selectCallExpression);

            string first = selectQueryProvider.Execute<string>(firstCallExpression).WriteLine(); // Execute query.
        }

        internal static void CompileSelectAndFirstExpressions(AdventureWorks adventureWorks)
        {
            var selectQueryable = adventureWorks.Products
                .Select(product => product.Name).Expression;
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression linqExpression = Expression.Call(
                method: firstMethod.Method, arg0: selectQueryable);
            (SelectExpression DatabaseExpression, IReadOnlyDictionary<string, object> Parameters) compilation =
                adventureWorks.Compile(linqExpression);
            compilation.DatabaseExpression.WriteLine();
            compilation.Parameters.WriteLines(parameter => $"{parameter.Key}: {parameter.Value}");
        }

        internal static void SelectAndFirstQuery(AdventureWorks adventureWorks)
        {
            string first = adventureWorks.Products.Select(product => product.Name).First();
            // Equivalent to:
            // string first = (from product in adventureWorks.Products select product.Name).First();
        }

        internal static SelectExpression SelectAndFirstDatabaseExpressions(AdventureWorks adventureWorks)
        {
            IQueryableMethodTranslatingExpressionVisitorFactory expressionVisitorFactory = adventureWorks
                .GetService<IQueryableMethodTranslatingExpressionVisitorFactory>();
            ISqlExpressionFactory expressionFactory = adventureWorks.GetService<ISqlExpressionFactory>();
            IEntityType entityType = adventureWorks.Model.FindEntityType(typeof(Product));
            SelectExpression databaseExpression = expressionFactory.Select(entityType);
            EntityProjectionExpression projectionExpression = (EntityProjectionExpression)databaseExpression.GetMappedProjection(new ProjectionMember());
            ColumnExpression columnExpression = projectionExpression.BindProperty(entityType.FindProperty(nameof(Product.Name)));
            databaseExpression.AddToProjection(columnExpression);
            databaseExpression.ApplyLimit(expressionFactory.ApplyDefaultTypeMapping(new SqlConstantExpression(Expression.Constant(1), null)));
            return databaseExpression.WriteLine();
        }

        public static (SelectExpression, IReadOnlyDictionary<string, object>) Compile(
            this DbContext dbContext, Expression linqExpression)
        {
            QueryContext queryContext = dbContext
                .GetService<IQueryContextFactory>()
                .Create();
            QueryCompilationContext compilationContext = dbContext
                .GetService<IQueryCompilationContextFactory>()
                .Create(async: false);
            QueryCompiler queryCompiler = (QueryCompiler)dbContext.GetService<IQueryCompiler>();
            linqExpression = queryCompiler.ExtractParameters(
                linqExpression,
                queryContext, 
                dbContext.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>());
            linqExpression = dbContext
                .GetService<IQueryTranslationPreprocessorFactory>()
                .Create(compilationContext)
                .Process(linqExpression);
            ShapedQueryExpression queryExpression = (ShapedQueryExpression)dbContext
                .GetService<IQueryableMethodTranslatingExpressionVisitorFactory>()
                .Create(dbContext.Model)
                .Visit(linqExpression);
            queryExpression = (ShapedQueryExpression)dbContext
                .GetService<IQueryTranslationPostprocessorFactory>()
                .Create(compilationContext)
                .Process(queryExpression);
            return ((SelectExpression)queryExpression.QueryExpression, queryContext.ParameterValues);
        }

        public static IRelationalCommand Generate(this DbContext dbContext, SelectExpression databaseExpression)
        {
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            QuerySqlGenerator sqlGenerator = sqlGeneratorFactory.Create();
            return sqlGenerator.GetCommand(databaseExpression);
        }

        public static IEnumerable<TEntity> MaterializeEntity<TEntity>(
            this DbContext dbContext,
            IRelationalCommand sql,
            IReadOnlyDictionary<string, object> parameters = null)
            where TEntity : class
        {
            return dbContext.Set<TEntity>().FromSqlRaw(
                sql: sql.CommandText,
                parameters: parameters.Select(parameter => new SqlParameter(parameter.Key, parameter.Value)).ToArray());
        }

        private static bool FilterName(string name) => name.Length > 10;

        internal static void WhereAndSelectWithCustomPredicate(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> products = source
                .Where(product => FilterName(product.Name))
                .Select(product => product.Name); // Define query.
            products.WriteLines(); // Execute query.
            // InvalidOperationException: The LINQ expression 'DbSet<Product>.Where(p => Translation.FilterName(p.Name))' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to either AsEnumerable(), AsAsyncEnumerable(), ToList(), or ToListAsync(). See https://go.microsoft.com/fwlink/?linkid=2101038 for more information.
        }

        internal static void WhereAndSelectWithLocalPredicate(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IEnumerable<string> products = source
                .Select(product => product.Name) // LINQ to Entities.
                .AsEnumerable() // LINQ to Objects.
                .Where(name => FilterName(name)); // Define query, IEnumerable<string> instead of IQueryable<string>.
            products.WriteLines(); // Execute query.
        }

        internal static void DatabaseFunction(AdventureWorks adventureWorks)
        {
            var photos = adventureWorks.ProductPhotos.Select(photo => new
            {
                LargePhotoFileName = photo.LargePhotoFileName,
                UnmodifiedDays = EF.Functions.DateDiffDay(photo.ModifiedDate, DateTime.UtcNow)
            }); // Define query.
            photos.WriteLines(); // Execute query.
            // SELECT [photo].[LargePhotoFileName], DATEDIFF(DAY, [photo].[ModifiedDate], GETUTCDATE()) AS [UnmodifiedDays]
            // FROM [Production].[ProductPhoto] AS [photo]
        }

        internal static void DatabaseOperator(AdventureWorks adventureWorks)
        {
            IQueryable<string> products = adventureWorks.Products
                .Select(product => product.Name)
                .Where(name => EF.Functions.Like(name, "%Touring%50%")); // Define query.
            products.WriteLines(); // Execute query.
            // SELECT [product].[Name]
            // FROM [Production].[Product] AS [product]
            // WHERE [product].[Name] LIKE N'%Touring%50%'
        }

        internal static void WhereAndSelectSql(AdventureWorks adventureWorks)
        {
            SelectExpression databaseExpression = WhereAndSelectDatabaseExpressions(adventureWorks);
            IRelationalCommand sql = adventureWorks.Generate(databaseExpression);
            sql.CommandText.WriteLine();
            // SELECT [p].[Name]
            // FROM [Production].[Product] AS [p]
            // WHERE CAST(LEN([p].[Name]) AS int) > 10
        }

        internal static void SelectAndFirstSql(AdventureWorks adventureWorks)
        {
            SelectExpression databaseExpression = SelectAndFirstDatabaseExpressions(adventureWorks);
            IRelationalCommand sql = adventureWorks.Generate(databaseExpression);
            sql.CommandText.WriteLine();
            // SELECT TOP(1) [p].[Name]
            // FROM [Production].[Product] AS [p]
        }
    }

#if DEMO
    public class LogConfiguration : DbConfiguration
    {
        public LogConfiguration()
        {
            this.SetProviderServices(SqlProviderServices.ProviderInvariantName, new LogProviderServices());
        }
    }
#endif
}

#if DEMO
namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Remotion.Linq.Clauses;

    public class SelectExpression : TableExpressionBase
    {
        public virtual IReadOnlyList<Expression> Projection { get; } // SELECT.

        public virtual bool IsDistinct { get; set; } // DISTINCT.

        public virtual Expression Limit { get; set; } // TOP.

        public virtual IReadOnlyList<TableExpressionBase> Tables { get; } // FROM.

        public virtual Expression Predicate { get; set; } // WHERE.

        public virtual IReadOnlyList<Ordering> OrderBy { get; } // ORDER BY.

        public virtual Expression Offset { get; set; } // OFFSET.

        public override Type Type { get; }

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    using System.Linq.Expressions;

    using Microsoft.EntityFrameworkCore.Query.Expressions;

    public class SqlServerStringLengthTranslator : IMemberTranslator
    {
        public virtual Expression Translate(MemberExpression memberExpression) =>
            memberExpression.Expression != null
            && memberExpression.Expression.Type == typeof(string)
            && memberExpression.Member.Name == nameof(string.Length)
                ? new SqlFunctionExpression("LEN", memberExpression.Type, new Expression[] { memberExpression.Expression })
                : null;
    }
}

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore.Storage;

    public interface IQuerySqlGenerator
    {
        IRelationalCommand GenerateSql(IReadOnlyDictionary<string, object> parameterValues);

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore.Storage
{
    using System.Collections.Generic;

    public interface IRelationalCommand
    {
        string CommandText { get; }

        IReadOnlyList<IRelationalParameter> Parameters { get; }

        RelationalDataReader ExecuteReader(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues);

        // Other members.
    }
}

namespace System.Data.Common
{
    public abstract class DbCommand : Component, IDbCommand, IDisposable
    {
        public abstract string CommandText { get; set; }

        public DbParameterCollection Parameters { get; }

        public DbDataReader ExecuteReader();

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable
    {
        // Expression Expression { get; } from IQueryable.

        // Type ElementType { get; } from IQueryable.

        // IQueryProvider Provider { get; } from IQueryable.

        // IEnumerator<T> GetEnumerator(); from IEnumerable<T>.
    }
}

namespace System.Linq
{
    using System.Linq.Expressions;

    public interface IQueryProvider
    {
        IQueryable CreateQuery(Expression expression);

        IQueryable<TElement> CreateQuery<TElement>(Expression expression);

        object Execute(Expression expression);

        TResult Execute<TResult>(Expression expression);
    }
}

namespace System.Linq
{
    using System.Linq.Expressions;
    using System.Reflection;

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, IQueryable<TSource>> currentMethod =
                Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression,
                arg1: Expression.Quote(predicate));
            return source.Provider.CreateQuery<TSource>(whereCallExpression);
        }

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, TResult>>, IQueryable<TResult>> currentMethod =
                Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression,
                arg1: Expression.Quote(selector));
            return source.Provider.CreateQuery<TResult>(selectCallExpression);
        }

        public static TSource First<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource> currentMethod = First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression,
                arg1: Expression.Quote(predicate));
            return source.Provider.Execute<TSource>(firstCallExpression);
        }

        public static TSource First<TSource>(this IQueryable<TSource> source)
        {
            Func<IQueryable<TSource>, TSource> currentMethod = First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression);
            return source.Provider.Execute<TSource>(firstCallExpression);
        }

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore
{
    public static class EF
    {
        public static DbFunctions Functions { get; }
        
        // Other members.
    }
}

// Microsoft.EntityFrameworkCore.dll
namespace Microsoft.EntityFrameworkCore
{
    public static class DbFunctionsExtensions
    {
        public static bool Like(this DbFunctions _, string matchExpression, string pattern);
        
        // Other members.
    }
}

// Microsoft.EntityFrameworkCore.SqlServer.dll
namespace Microsoft.EntityFrameworkCore
{
    public static class SqlServerDbFunctionsExtensions
    {
        public static bool Contains(this DbFunctions _, string propertyReference, string searchCondition);

        public static int DateDiffDay(this DbFunctions _, DateTime startDate, DateTime endDate);

        // Other members.
    }
}
#endif
