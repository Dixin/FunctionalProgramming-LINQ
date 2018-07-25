namespace Tutorial.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
#if !__IOS__
    using System.Reflection;
    using System.Reflection.Emit;
#endif

    internal static partial class RemoteQueries
    {
        internal static string InOrder(this LambdaExpression expression)
        {
            string VisitNode(Expression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Constant when node is ConstantExpression constant:
                        return constant.Value.ToString();

                    case ExpressionType.Parameter when node is ParameterExpression parameter:
                        return $"@{parameter.Name}";

                    // In-order output: left child, current node, right child.
                    case ExpressionType.Add when node is BinaryExpression binary:
                        return $"({VisitNode(binary.Left)} + {VisitNode(binary.Right)})";

                    case ExpressionType.Subtract when node is BinaryExpression binary:
                        return $"({VisitNode(binary.Left)} - {VisitNode(binary.Right)})";

                    case ExpressionType.Multiply when node is BinaryExpression binary:
                        return $"({VisitNode(binary.Left)} * {VisitNode(binary.Right)})";

                    case ExpressionType.Divide when node is BinaryExpression binary:
                        return $"({VisitNode(binary.Left)} / {VisitNode(binary.Right)})";

                    default:
                        throw new ArgumentOutOfRangeException(nameof(expression));
                }
            }

            return $"SELECT {VisitNode(expression.Body)};";
        }

        internal static void Infix()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            string sql = expression.InOrder();
            sql.WriteLine(); // SELECT (((@a + @b) - ((@c * @d) / 2)) + (@e * 3));
        }

        internal static double ExecuteSql(
            string connection,
            string sql,
            IDictionary<string, double> parameters)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
            {
                sqlConnection.Open();
                parameters.ForEach(parameter => sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value));
                return (double)sqlCommand.ExecuteScalar();
            }
        }

#if !__IOS__
        public static TDelegate TranslateToSql<TDelegate>(this Expression<TDelegate> expression, string connection)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(
                string.Empty,
                expression.ReturnType,
                expression.Parameters.Select(parameter => parameter.Type).ToArray(),
                MethodBase.GetCurrentMethod().Module);
            EmitCil(dynamicMethod.GetILGenerator(), expression.InOrder());
            return (TDelegate)(object)dynamicMethod.CreateDelegate(typeof(TDelegate));

            void EmitCil(ILGenerator generator, string sql)
            {
                // Dictionary<string, double> dictionary = new Dictionary<string, double>();
                generator.DeclareLocal(typeof(Dictionary<string, double>));
                generator.Emit(
                    OpCodes.Newobj,
                    typeof(Dictionary<string, double>).GetConstructor(Array.Empty<Type>()));
                generator.Emit(OpCodes.Stloc_0);

                for (int index = 0; index < expression.Parameters.Count; index++)
                {
                    // dictionary.Add($"@{expression.Parameters[i].Name}", args[i]);
                    generator.Emit(OpCodes.Ldloc_0); // dictionary.
                    generator.Emit(OpCodes.Ldstr, $"@{expression.Parameters[index].Name}");
                    generator.Emit(OpCodes.Ldarg_S, index);
                    generator.Emit(
                        OpCodes.Callvirt,
                        typeof(Dictionary<string, double>).GetMethod(
                            nameof(Dictionary<string, double>.Add),
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod));
                }

                // ExecuteSql(connection, expression, dictionary);
                generator.Emit(OpCodes.Ldstr, connection);
                generator.Emit(OpCodes.Ldstr, sql);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(
                    OpCodes.Call,
                    new Func<string, string, IDictionary<string, double>, double>(ExecuteSql).Method);

                generator.Emit(OpCodes.Ret); // Returns the result.
            }
        }
#endif

        internal static void TranslateAndExecute()
        {
            Expression<Func<double, double, double, double, double, double>> expression2 =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            Func<double, double, double, double, double, double> local = expression2.Compile();
            local(1, 2, 3, 4, 5).WriteLine(); // 12
#if !__IOS__
            Func<double, double, double, double, double, double> remote = expression2.TranslateToSql(ConnectionStrings.AdventureWorks);
            remote(1, 2, 3, 4, 5).WriteLine(); // 12
#endif
        }
    }
}

#if DEMO
namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IQueryable : IEnumerable
    {
        Expression Expression { get; }

        Type ElementType { get; }

        IQueryProvider Provider { get; }
    }

    public interface IOrderedQueryable : IQueryable, IEnumerable { }

    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable { }

    public interface IOrderedQueryable<out T> : IQueryable<T>, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable { }
}

namespace System.Linq
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);

        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source);

        // Other members.
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector);

        public static IQueryable<TSource> Concat<TSource>(
            this IQueryable<TSource> source1, IEnumerable<TSource> source2);

        public static IQueryable<TResult> Cast<TResult>(this IQueryable source);

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class Enumerable
    {
        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);
    }

    public static class Queryable
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> OrderByDescending<TSource, TKey>(
            this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
            this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);

        public static IOrderedQueryable<TSource> ThenByDescending<TSource, TKey>(
            this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector);
    }
}
#endif
