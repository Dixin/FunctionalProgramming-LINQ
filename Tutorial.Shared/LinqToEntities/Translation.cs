namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.Expressions;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Microsoft.EntityFrameworkCore.Query.Sql;
    using Microsoft.EntityFrameworkCore.Storage;

    using Remotion.Linq;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
    using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
    using Remotion.Linq.Parsing.Structure;
    using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;

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
            QueryCompilationContext compilationContext = adventureWorks.GetService<IQueryCompilationContextFactory>()
                .Create(async: false);
            SelectExpression databaseExpression = new SelectExpression(
                dependencies: new SelectExpressionDependencies(adventureWorks.GetService<IQuerySqlGeneratorFactory>(), adventureWorks.GetService<IRelationalTypeMappingSource>()),
                queryCompilationContext: (RelationalQueryCompilationContext)compilationContext);
            MainFromClause querySource = new MainFromClause(
                itemName: "product",
                itemType: typeof(Product),
                fromExpression: Expression.Constant(adventureWorks.ProductCategories));
            TableExpression tableExpression = new TableExpression(
                table: nameof(Product),
                schema: AdventureWorks.Production,
                alias: querySource.ItemName,
                querySource: querySource);
            databaseExpression.AddTable(tableExpression);
            IEntityType productEntityType = adventureWorks.Model.FindEntityType(typeof(Product));
            IProperty nameProperty = productEntityType.FindProperty(nameof(Product.Name));
            ColumnExpression nameColumn = new ColumnExpression(
                name: nameof(Product.Name), property: nameProperty, tableExpression: tableExpression);
            databaseExpression.AddToProjection(nameColumn);
            databaseExpression.AddToPredicate(Expression.GreaterThan(
                left: new ExplicitCastExpression(
                    operand: new SqlFunctionExpression(
                        functionName: "LEN",
                        returnType: typeof(int),
                        arguments: new Expression[] { nameColumn }),
                    type: typeof(int)),
                right: Expression.Constant(10)));
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
            QueryCompilationContext compilationContext = adventureWorks.GetService<IQueryCompilationContextFactory>()
                .Create(async: false);
            SelectExpression selectExpression = new SelectExpression(
                dependencies: new SelectExpressionDependencies(adventureWorks.GetService<IQuerySqlGeneratorFactory>(), adventureWorks.GetService<IRelationalTypeMappingSource>()),
                queryCompilationContext: (RelationalQueryCompilationContext)compilationContext);
            MainFromClause querySource = new MainFromClause(
                itemName: "product",
                itemType: typeof(Product),
                fromExpression: Expression.Constant(adventureWorks.ProductCategories));
            TableExpression tableExpression = new TableExpression(
                table: nameof(Product),
                schema: AdventureWorks.Production,
                alias: querySource.ItemName,
                querySource: querySource);
            selectExpression.AddTable(tableExpression);
            IEntityType productEntityType = adventureWorks.Model.FindEntityType(typeof(Product));
            IProperty nameProperty = productEntityType.FindProperty(nameof(Product.Name));
            selectExpression.AddToProjection(new ColumnExpression(
                name: nameof(Product.Name), property: nameProperty, tableExpression: tableExpression));
            selectExpression.Limit = Expression.Constant(1);
            return selectExpression.WriteLine();
        }

        public static (SelectExpression, IReadOnlyDictionary<string, object>) Compile(
            this DbContext dbContext, Expression linqExpression)
        {
            QueryContext queryContext = dbContext.GetService<IQueryContextFactory>().Create();
            IEvaluatableExpressionFilter evaluatableExpressionFilter = dbContext.GetService<IEvaluatableExpressionFilter>();
            linqExpression = new ParameterExtractingExpressionVisitor(
                evaluatableExpressionFilter: evaluatableExpressionFilter,
                parameterValues: queryContext,
                logger: dbContext.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>(),
                context: dbContext,
                parameterize: true).ExtractParameters(linqExpression);
            QueryParser queryParser = new QueryParser(new ExpressionTreeParser(
                nodeTypeProvider: dbContext.GetService<INodeTypeProviderFactory>().Create(),
                processor: new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                {
                    new PartialEvaluatingExpressionTreeProcessor(evaluatableExpressionFilter),
                    new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                })));
            QueryModel queryModel = queryParser.GetParsedQuery(linqExpression);

            Type resultType = queryModel.GetResultType();
            if (resultType.IsConstructedGenericType && resultType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                resultType = resultType.GenericTypeArguments.Single();
            }

            QueryCompilationContext compilationContext = dbContext.GetService<IQueryCompilationContextFactory>()
                .Create(async: false);
            RelationalQueryModelVisitor queryModelVisitor = (RelationalQueryModelVisitor)compilationContext
                .CreateQueryModelVisitor();
            queryModelVisitor.GetType()
                .GetMethod(nameof(RelationalQueryModelVisitor.CreateQueryExecutor))
                .MakeGenericMethod(resultType)
                .Invoke(queryModelVisitor, new object[] { queryModel });
            SelectExpression databaseExpression = queryModelVisitor.TryGetQuery(queryModel.MainFromClause);
            databaseExpression.QuerySource = queryModel.MainFromClause;
            return (databaseExpression, queryContext.ParameterValues);
        }

        public partial class ApiCompilationFilter : EvaluatableExpressionFilterBase
        {
            private static readonly PropertyInfo DateTimeUtcNow = typeof(DateTime)
                .GetProperty(nameof(DateTime.UtcNow));

            public override bool IsEvaluatableMember(MemberExpression memberExpression) =>
                memberExpression.Member != DateTimeUtcNow;
        }

        public static IRelationalCommand Generate(
            this DbContext dbContext,
            SelectExpression databaseExpression,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            IQuerySqlGenerator sqlGenerator = sqlGeneratorFactory.CreateDefault(databaseExpression);
            return sqlGenerator.GenerateSql(parameters ?? new Dictionary<string, object>());
        }

        public static IEnumerable<TResult> Materialize<TResult>(
            this DbContext dbContext,
            SelectExpression databaseExpression,
            IRelationalCommand sql,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            Func<DbDataReader, TResult> materializer = dbContext.GetMaterializer<TResult>(databaseExpression, parameters);
            using (RelationalDataReader reader = sql.ExecuteReader(
                connection: dbContext.GetService<IRelationalConnection>(), parameterValues: parameters))
            {
                while (reader.DbDataReader.Read())
                {
                    yield return materializer(reader.DbDataReader);
                }
            }
        }

        public static Func<DbDataReader, TResult> GetMaterializer<TResult>(
            this DbContext dbContext,
            SelectExpression databaseExpression,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            IMaterializerFactory materializerFactory = dbContext.GetService<IMaterializerFactory>();
            Func<ValueBuffer, object> materializer = (Func<ValueBuffer, object>)materializerFactory
                .CreateMaterializer(
                    entityType: dbContext.Model.FindEntityType(typeof(TResult)),
                    selectExpression: databaseExpression,
                    projectionAdder: (property, expression) => expression.AddToProjection(
                        property, databaseExpression.QuerySource),
                    typeIndexMap: out _)
                .Compile();
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            IQuerySqlGenerator sqlGenerator = sqlGeneratorFactory.CreateDefault(databaseExpression);
            IRelationalValueBufferFactoryFactory valueBufferFactory = dbContext.GetService<IRelationalValueBufferFactoryFactory>();
            return dbReader => (TResult)materializer(sqlGenerator.CreateValueBufferFactory(valueBufferFactory, dbReader).Create(dbReader));
        }

        public static IEnumerable<TEntity> MaterializeEntity<TEntity>(
            this DbContext dbContext,
            IRelationalCommand sql,
            IReadOnlyDictionary<string, object> parameters = null)
            where TEntity : class
        {
            return dbContext.Set<TEntity>().FromSql(
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
            // SELECT [product].[Name]
            // FROM [Production].[Product] AS [product]
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
            IRelationalCommand sql = adventureWorks.Generate(databaseExpression: databaseExpression, parameters: null);
            sql.CommandText.WriteLine();
            // SELECT [product].[Name]
            // FROM [Production].[ProductCategory] AS [product]
            // WHERE CAST(LEN([product].[Name]) AS int) > 10
        }

        internal static void SelectAndFirstSql(AdventureWorks adventureWorks)
        {
            SelectExpression databaseExpression = SelectAndFirstDatabaseExpressions(adventureWorks);
            IRelationalCommand sql = adventureWorks.Generate(databaseExpression: databaseExpression, parameters: null);
            sql.CommandText.WriteLine();
            // SELECT TOP(1) [product].[Name]
            // FROM [Production].[Product] AS [product]
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
