namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    internal class Product
    {
        internal int ProductID { get; set; }

        internal string Name { get; set; }

        internal decimal ListPrice { get; set; }

        internal int? ProductSubcategoryID { get; set; }
    }

    internal static partial class ExpressionTrees
    {
        internal static void ExpressionLambda()
        {
            Expression<Func<int, bool>> isPositiveExpression = int32 => int32 > 0;
            // Compare to: Func<int, bool> isPositive = int32 => int32 > 0;
        }

        internal static void CompiledExpressionLambda()
        {
            ParameterExpression parameterExpression = Expression.Parameter(type: typeof(int), name: "int32"); // int32 parameter.
            ConstantExpression constantExpression = Expression.Constant(value: 0, type: typeof(int)); // 0
            BinaryExpression greaterThanExpression = Expression.GreaterThan(
                left: parameterExpression, right: constantExpression); // int32 > 0

            Expression<Func<int, bool>> isPositiveExpression = Expression.Lambda<Func<int, bool>>(
                body: greaterThanExpression, // ... => int32 > 0
                parameters: parameterExpression); // int32 => ...
        }

#if DEMO
        internal static void StatementLambda()
        {
            Expression<Func<int, bool>> isPositiveExpression = int32 =>
            {
                Console.WriteLine(int32);
                return int32 > 0;
            }; // Cannot be compiled.
        }
#endif

        internal static void StatementLambda()
        {
            ParameterExpression parameterExpression = Expression.Parameter(type: typeof(int), name: "int32"); // int32 parameter.
            Expression<Func<int, bool>> isPositiveExpression = Expression.Lambda<Func<int, bool>>(
                body: Expression.Block( // ... => {
                                        // Console.WriteLine(int32);
                    arg0: Expression.Call(method: new Action<int>(Console.WriteLine).Method, arg0: parameterExpression),
                    // return int32 > 0;
                    arg1: Expression.GreaterThan(left: parameterExpression, right: Expression.Constant(value: 0, type: typeof(int)))), // }
                parameters: parameterExpression); // int32 => ...
        }

        internal static void Infix()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
        }

        internal static string PreOrderOutput(this LambdaExpression expression)
        {
            string VisitNode(Expression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                        BinaryExpression binary = (BinaryExpression)node;
                        // Pre-order output: current node, left child, right child.
                        return $"{binary.NodeType}({VisitNode(binary.Left)}, {VisitNode(binary.Right)})";

                    case ExpressionType.Constant:
                        return ((ConstantExpression)node).Value.ToString();

                    case ExpressionType.Parameter:
                        return ((ParameterExpression)node).Name;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(node));
                }
            }

            return VisitNode(expression.Body);
        }

        internal static void Prefix()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            string prefix = infix.PreOrderOutput();
            prefix.WriteLine(); // Add(Subtract(Add(a, b), Divide(Multiply(c, d), 2)), Multiply(e, 3))
        }

        internal static List<(OpCode, object)> PostOrderOutput(this LambdaExpression expression)
        {
            List<(OpCode, object)> VisitNode(Expression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                        return VisitBinary((BinaryExpression)node, OpCodes.Add);

                    case ExpressionType.Subtract:
                        return VisitBinary((BinaryExpression)node, OpCodes.Sub);

                    case ExpressionType.Multiply:
                        return VisitBinary((BinaryExpression)node, OpCodes.Mul);

                    case ExpressionType.Divide:
                        return VisitBinary((BinaryExpression)node, OpCodes.Div);

                    case ExpressionType.Constant:
                        return new List<(OpCode, object)>()
                        {
                            (OpCodes.Ldc_R8, ((ConstantExpression)node).Value) // Push constant to stack.
                        };

                    case ExpressionType.Parameter:
                        int parameterIndex = expression.Parameters.IndexOf((ParameterExpression)node);
                        return new List<(OpCode, object)>()
                        {
                            (OpCodes.Ldarg_S, parameterIndex) // Push parameter of the specified index to stack.
                        };

                    default:
                        throw new ArgumentOutOfRangeException(nameof(node));
                }
            }

            List<(OpCode, object)> VisitBinary(BinaryExpression binary, OpCode postfix)
            {
                // Post-order output: left child, right child, current node.
                List<(OpCode, object)> instructions = VisitNode(binary.Left);
                instructions.AddRange(VisitNode(binary.Right));
                instructions.Add((postfix, null)); // Operate and push the result to stack.
                return instructions;
            }

            return VisitNode(expression.Body);
        }

        internal static void Postfix()
        {
            Expression<Func<double, double, double, double, double, double>> infix =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            List<(OpCode, object)> postfix = infix.PostOrderOutput();
            foreach ((OpCode instruction, object argument) in postfix)
            {
                $"{instruction} {argument}".WriteLine();
            }
            // ldarg.s 0
            // ldarg.s 1
            // add
            // ldarg.s 2
            // ldarg.s 3 
            // mul 
            // ldc.r8 2 
            // div 
            // sub 
            // ldarg.s 4 
            // ldc.r8 3 
            // mul 
            // add
        }

#if !__IOS__
        internal static TDelegate CompileToCil<TDelegate>(this Expression<TDelegate> expression)
        {
            DynamicMethod dynamicFunction = new DynamicMethod(
                name: string.Empty,
                returnType: expression.ReturnType,
                parameterTypes: expression.Parameters.Select(parameter => parameter.Type).ToArray(),
                m: MethodBase.GetCurrentMethod().Module);
            EmitCil(dynamicFunction.GetILGenerator(), expression.PostOrderOutput());
            return (TDelegate)(object)dynamicFunction.CreateDelegate(typeof(TDelegate));

            void EmitCil(ILGenerator generator, List<(OpCode, object)> cil)
            {
                foreach ((OpCode instruction, object arg) in cil)
                {
                    if (arg == null)
                    {
                        generator.Emit(instruction); // add, sub, mul, div has no argument.
                    }
                    else if (arg is int)
                    {
                        generator.Emit(instruction, (int)arg); // ldarg.s has int argument of parameter index.
                    }
                    else if (arg is double)
                    {
                        generator.Emit(instruction, (double)arg); // ldc.r8 has double argument of constant.
                    }
                }
                generator.Emit(OpCodes.Ret); // Return the result.
            }
        }

        internal static void CompileAndRun()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            Func<double, double, double, double, double, double> function = expression.CompileToCil();
            double result = function(1D, 2D, 3D, 4D, 5D);
            result.WriteLine(); // 12
        }

        internal static void BuiltInCompile()
        {
            Expression<Func<double, double, double, double, double, double>> expression =
                (a, b, c, d, e) => a + b - c * d / 2D + e * 3D;
            Func<double, double, double, double, double, double> function = expression.Compile();
            double result = function(1D, 2D, 3D, 4D, 5D); // 12
        }
#endif

        internal static void LinqToObjectsQuery(IEnumerable<Product> source) // Get source.
        {
            IEnumerable<Product> query = source.Where(product => product.ListPrice > 0M); // Define query.
            foreach (Product result in query) // Execute query.
            {
                result.Name.WriteLine();
            }
        }

        internal static void LinqToEntitiesQuery(IQueryable<Product> source) // Get source.
        {
            IQueryable<Product> query = source.Where(product => product.ListPrice > 0M); // Define query.
            foreach (Product result in query) // Execute query.
            {
                result.Name.WriteLine();
            }
        }

        internal static void EquivalentLinqToObjectsQuery(IEnumerable<Product> source) // Get source.
        {
            Func<Product, bool> predicateFunction = product => product.ListPrice > 0M;
            // Compiled to named function with cache field.

            IEnumerable<Product> query = Enumerable.Where(source, predicateFunction); // Define query.

            foreach (Product result in query) // Execute query.
            {
                result.Name.WriteLine();
            }
        }

        internal static void EquivalentLinqToEntitiesQuery(IQueryable<Product> source) // Get source.
        {
            Expression<Func<Product, bool>> predicateExpression = product => product.ListPrice > 0M;
            // Compiled to:
            // ParameterExpression productParameter = Expression.Parameter(type: typeof(Product), name: "product");
            // Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
            //    body: Expression.GreaterThan(
            //        left: Expression.Property(expression: productParameter, propertyName: nameof(Product.ListPrice)),
            //        right: Expression.Constant(value: 0M, type: typeof(decimal))),
            //    productParameter);

            IQueryable<Product> query = Queryable.Where(source, predicateExpression); // Define query.

            foreach (Product result in query) // Execute query.
            {
                result.Name.WriteLine();
            }
        }

        [CompilerGenerated]
        private static Func<Product, bool> cachedPredicate;

        [CompilerGenerated]
        private static bool CompiledPredicate(Product product) => product.ListPrice > 0M;

        public static void CompiledLinqToObjects(IEnumerable<Product> source)
        {
            Func<Product, bool> predicate = cachedPredicate ?? (cachedPredicate = CompiledPredicate);
            IEnumerable<Product> query = Enumerable.Where(source, predicate);
            foreach (Product result in query) // Execute query.
            {
                TraceExtensions.WriteLine(result.Name);
            }
        }

        internal static void CompiledLinqToEntities(IQueryable<Product> source)
        {
            ParameterExpression productParameter = Expression.Parameter(type: typeof(Product), name: "product");
            Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                body: Expression.GreaterThan(
                    left: Expression.Property(expression: productParameter, propertyName: nameof(Product.ListPrice)),
                    right: Expression.Constant(value: 0M, type: typeof(decimal))),
                productParameter);

            IQueryable<Product> query = Queryable.Where(source: source, predicate: predicateExpression); // Define query.
            foreach (Product result in query) // Execute query.
            {
                TraceExtensions.WriteLine(value: result.Name);
            }
        }
    }
}

#if DEMO
namespace System.Linq.Expressions
{
    using System.Collections.ObjectModel;
    using System.Reflection;

    public abstract partial class Expression
    {
        public virtual ExpressionType NodeType { get; }

        public virtual Type Type { get; }

        // Other members.
    }

    public class ParameterExpression : Expression
    {
        public string Name { get; }

        // Other members.
    }

    public class ConstantExpression : Expression
    {
        public object Value { get; }

        // Other members.
    }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; }

        public Expression Right { get; }

        // Other members.
    }

    public abstract class LambdaExpression : Expression
    {
        public Expression Body { get; }

        public ReadOnlyCollection<ParameterExpression> Parameters { get; }

        // Other members.
    }

    public sealed class Expression<TDelegate> : LambdaExpression
    {
        public TDelegate Compile();

        // Other members.
    }

    public abstract partial class Expression
    {
        public static ParameterExpression Parameter(Type type, string name);

        public static ConstantExpression Constant(object value, Type type);

        public static BinaryExpression GreaterThan(Expression left, Expression right);

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters);
    }

    public abstract partial class Expression
    {
        public static BinaryExpression Add(Expression left, Expression right);

        public static BinaryExpression Subtract(Expression left, Expression right);

        public static BinaryExpression Multiply(Expression left, Expression right);

        public static BinaryExpression Divide(Expression left, Expression right);

        public static BinaryExpression Equal(Expression left, Expression right);

        public static UnaryExpression ArrayLength(Expression array);

        public static UnaryExpression Not(Expression expression);

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse);

        public static NewExpression New(ConstructorInfo constructor, params Expression[] arguments);

        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments);

        public static BlockExpression Block(params Expression[] expressions);

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate);
    }
}
#endif
