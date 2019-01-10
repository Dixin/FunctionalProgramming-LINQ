namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;

    internal static partial class Compositions
    {
        internal static void LinqComposition()
        {
            int queryResultCount = Enumerable
                .Repeat(0, 10) // -> IEnumerable<int>
                .Select(int32 => Path.GetRandomFileName()) // -> IEnumerable<string>
                .OrderByDescending(@string => @string.Length) // -> IOrderedEnumerable<string>
                .ThenBy(@string => @string) // -> IOrderedEnumerable<string>
                .Select(@string => $"{@string.Length}: {@string}") // -> IEnumerable<string>
                .Count(); // -> int
        }

        internal static void CompiledLinqComposition()
        {
            string firstQueryResult =
                Enumerable.First(
                    Enumerable.Select(
                        Enumerable.ThenBy(
                            Enumerable.OrderByDescending(
                                Enumerable.Select(
                                    Enumerable.Repeat(0, 10),
                                    int32 => Path.GetRandomFileName()
                                ),
                                @string => @string.Length
                            ),
                            @string => @string
                        ),
                        @string => $"{@string.Length}: {@string}"
                    )
                );
        }

        internal static void RangeVariable(IEnumerable<int> source)
        {
            IEnumerable<string> query =
                from variable in source // variable is int.
                where variable > 0 // variable is int.
                select variable.ToString() /* variable is int. */ into variable // variable is string.
                orderby variable.Length // variable is string.
                select variable.ToUpperInvariant(); // variable is string.
        }

        internal static void QueryExpressionClause(
            IEnumerable<int> int32Sequence, IEnumerable<string> stringSequence)
        {
            IEnumerable<int> singleFromWithSelect;
            singleFromWithSelect = from int32 in int32Sequence
                                   select int32;
            // IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector);
            singleFromWithSelect = int32Sequence.Select(int32 => int32);

            IEnumerable<string> let;
            let = from int32 in int32Sequence
                  let variable = int32 + 1
                  select variable.ToString();
            // IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector);
            let = int32Sequence
                .Select(int32 => new { int32, variable = int32 + 1 })
                .Select(context => context.variable.ToString());

            IEnumerable<int> multipleFromWithSelect;
            multipleFromWithSelect = from int32 in int32Sequence
                                     from @string in stringSequence
                                     select int32 + @string.Length;
            // IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector);
            multipleFromWithSelect = int32Sequence.SelectMany(
                int32 => stringSequence, (int32, @string) => int32 + @string.Length);

            IEnumerable<DayOfWeek> typeInFrom;
            typeInFrom = from DayOfWeek dayOfWeek in int32Sequence
                         select dayOfWeek;
            // IEnumerable<TResult> Cast<TResult>(this IEnumerable source);
            typeInFrom = int32Sequence.Cast<DayOfWeek>().Select(dayOfWeek => dayOfWeek);

            IEnumerable<int> where;
            where = from int32 in int32Sequence
                    where int32 > 0
                    select int32;
            // IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);
            where = int32Sequence.Where(int32 => int32 > 0);

            IOrderedEnumerable<string> orderByWithSingleKey;
            orderByWithSingleKey = from @string in stringSequence
                                   orderby @string.Length
                                   select @string;
            // IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);
            orderByWithSingleKey = stringSequence.OrderBy(@string => @string.Length);

            IOrderedEnumerable<string> orderByWithMultipleKeys;
            orderByWithMultipleKeys = from @string in stringSequence
                                      orderby @string.Length, @string descending
                                      select @string;
            // IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector);
            orderByWithMultipleKeys = stringSequence
                .OrderBy(@string => @string.Length).ThenByDescending(@string => @string);

            IEnumerable<IGrouping<int, int>> group;
            group = from int32 in int32Sequence
                    group int32 by int32 % 10;
            // IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);
            group = int32Sequence.GroupBy(int32 => int32 % 10);

            IEnumerable<IGrouping<int, string>> groupWithElementSelector;
            groupWithElementSelector = from int32 in int32Sequence
                                       group int32.ToString() by int32 % 10;
            // IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector);
            groupWithElementSelector = int32Sequence.GroupBy(int32 => int32 % 10, int32 => int32.ToString());

            IEnumerable<string> join;
            join = from int32 in int32Sequence
                   join @string in stringSequence on int32 equals @string.Length
                   select $"{int32} - {@string}";
            // IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector);
            join = int32Sequence.Join(
                stringSequence,
                int32 => int32,
                @string => @string.Length,
                (int32, @string) => $"{int32} - {@string}");

            IEnumerable<string> joinWithInto;
            joinWithInto = from int32 in int32Sequence
                           join @string in stringSequence on int32 equals @string.Length into stringGroup
                           select $"{int32} - {string.Join(", ", stringGroup)}";
            // IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector);
            joinWithInto = int32Sequence.GroupJoin(
                stringSequence,
                int32 => int32,
                @string => @string.Length,
                (int32, stringGroup) => $"{int32} - {string.Join(", ", stringGroup)}");

            IEnumerable<IGrouping<char, string>> intoWithContinuation;
            intoWithContinuation = from int32 in int32Sequence
                                   select Math.Abs(int32) into absolute
                                   select absolute.ToString() into @string
                                   group @string by @string[0];
            intoWithContinuation = int32Sequence
                .Select(int32 => Math.Abs(int32))
                .Select(absolute => absolute.ToString())
                .GroupBy(@string => @string[0]);
        }

        internal static void QueryExpression(
            IEnumerable<int> int32Sequence, IEnumerable<string> stringSequence)
        {
            IEnumerable<IGrouping<char, ConsoleColor>> query;
            query = from int32 in int32Sequence
                    from @string in stringSequence // SelectMany.
                    let length = @string.Length // Select.
                    where length > 1 // Where.
                    select int32 + length
                into sum // Select.
                    join ConsoleColor color in int32Sequence on sum % 15 equals (int)color // Join.
                    orderby color // OrderBy.
                    group color by color.ToString()[0]; // GroupBy.

            query = int32Sequence
                .SelectMany(int32 => stringSequence, (int32, @string) => new { int32, @string }) // Multiple from clauses.
                .Select(context => new { context, length = context.@string.Length }) // let clause.
                .Where(context => context.length > 1) // where clause.
                .Select(context => context.context.int32 + context.length) // select clause.
                .Join(
                    int32Sequence.Cast<ConsoleColor>(),
                    sum => sum % 15,
                    color => (int)color,
                    (sum, color) => new { sum, color }) // join clause without into.
                .OrderBy(context => context.color) // orderby clause.
                .GroupBy(
                    context => context.color.ToString()[0],
                    context => context.color); // group by clause without element selector.
        }

        internal static void CompiledQueryExpression(
            IEnumerable<int> int32Sequence, IEnumerable<string> stringSequence)
        {
            IEnumerable<IGrouping<char, ConsoleColor>> query =
                Enumerable.GroupBy(
                    Enumerable.OrderBy(
                        Enumerable.Join(
                            Enumerable.Select(
                                Enumerable.Where(
                                    Enumerable.Select(
                                        Enumerable.SelectMany(
                                            int32Sequence, int32 => stringSequence,
                                            (int32, @string) => new { int32, @string }),
                                        context => new { context, length = context.@string.Length }
                                    ),
                                    context => context.length > 1
                                ),
                                context => context.context.int32 + context.length
                            ),
                            Enumerable.Cast<ConsoleColor>(int32Sequence),
                            sum => sum % 15,
                            color => (int)color,
                            (sum, color) => new { sum, color }
                        ),
                        context => context.color
                    ),
                    context => context.color.ToString()[0],
                    context => context.color
                );
        }

        internal static void QueryExpressionToMethod(IEnumerable<int> source)
        {
            IEnumerable<int> positive = from int32 in source
                                        where int32 > 0
                                        select int32;
            foreach (int int32 in positive)
            {
            }
        }

        private static TResult Select<TResult>(this int source, Func<int, TResult> selector) =>
            selector(source);

        private static TResult Select<TResult>(this Guid source, Func<Guid, TResult> selector) =>
            selector(source);

        internal static void Select()
        {
            int defaultInt32;
            defaultInt32 = from zero in default(int)
                           select zero;
            defaultInt32 = Select(default(int), zero => zero);

            double squareRoot;
            squareRoot = from three in 1 + 2
                         select Math.Sqrt(three + 1);
            squareRoot = Select(1 + 2, three => Math.Sqrt(three + 1));

            string guidString;
            guidString = from guid in Guid.NewGuid()
                         select guid.ToString();
            guidString = Select(Guid.NewGuid(), guid => guid.ToString());
        }

        private static int? Where(this int source, Func<int, bool> predicate) =>
            predicate(source) ? (int?)source : null;

        private static TResult SelectMany<TSelector, TResult>(
            this Guid source, Func<Guid, TSelector> selector, Func<Guid, TSelector, TResult> resultSelector)
        {
            TSelector selectorResult = selector(source);
            return resultSelector(source, selectorResult);
        }

        internal static void WhereAndSelectMany()
        {
            int? positive;
            positive = from random in new Random().Next()
                       where random > 0
                       select random;
            positive = new Random().Next().Where(random => random > 0);

            string doubleGuidString;
            doubleGuidString = from guild1 in Guid.NewGuid()
                               from guid2 in Guid.NewGuid()
                               select guild1.ToString() + guid2.ToString();
            doubleGuidString = Guid.NewGuid().SelectMany(
                guild1 => Guid.NewGuid(), (guild1, guid2) => guild1.ToString() + guid2.ToString());
        }

        public interface ILocal
        {
            ILocal<T> Cast<T>();
        }

        public interface ILocal<T> : ILocal
        {
            ILocal<TResult> Select<TResult>(Func<T, TResult> selector); // select clause.

            ILocal<TResult> SelectMany<TSelector, TResult>(
                Func<T, ILocal<TSelector>> selector,
                Func<T, TSelector, TResult> resultSelector); // Multiple from clause.

            ILocal<T> Where(Func<T, bool> predicate); // where clause.

            IOrderedLocal<T> OrderBy<TKey>(Func<T, TKey> keySelector); // orderby clause.

            IOrderedLocal<T> OrderByDescending<TKey>(Func<T, TKey> keySelector); // orderby clause with descending.

            ILocal<ILocalGroup<TKey, T>> GroupBy<TKey>(Func<T, TKey> keySelector); // group clause without element selector.

            ILocal<ILocalGroup<TKey, TElement>> GroupBy<TKey, TElement>(
                Func<T, TKey> keySelector, Func<T, TElement> elementSelector); // group clause with element selector.

            ILocal<TResult> Join<TInner, TKey, TResult>(
                ILocal<TInner> inner,
                Func<T, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector,
                Func<T, TInner, TResult> resultSelector); // join clause.

            ILocal<TResult> GroupJoin<TInner, TKey, TResult>(
                ILocal<TInner> inner,
                Func<T, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector,
                Func<T, ILocal<TInner>, TResult> resultSelector); // join clause with into.
        }

        public interface IOrderedLocal<T> : ILocal<T>
        {
            IOrderedLocal<T> ThenBy<TKey>(Func<T, TKey> keySelector); // Multiple keys in orderby clause.

            IOrderedLocal<T> ThenByDescending<TKey>(Func<T, TKey> keySelector); // Multiple keys with descending in orderby clause.
        }

        public interface ILocalGroup<TKey, T>
        {
            TKey Key { get; }
        }

        public interface IRemote
        {
            IRemote<T> Cast<T>();
        }

        public interface IRemote<T> : IRemote
        {
            IRemote<TResult> Select<TResult>(Expression<Func<T, TResult>> selector);

            IRemote<TResult> SelectMany<TSelector, TResult>(
                Expression<Func<T, IRemote<TSelector>>> selector,
                Expression<Func<T, TSelector, TResult>> resultSelector);

            IRemote<T> Where(Expression<Func<T, bool>> predicate);

            IOrderedRemote<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

            IOrderedRemote<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);

            IRemote<IRemoteGroup<TKey, T>> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector);

            IRemote<IRemoteGroup<TKey, TElement>> GroupBy<TKey, TElement>(
                Expression<Func<T, TKey>> keySelector, Expression<Func<T, TElement>> elementSelector);

            IRemote<TResult> Join<TInner, TKey, TResult>(
                IRemote<TInner> inner,
                Expression<Func<T, TKey>> outerKeySelector,
                Expression<Func<TInner, TKey>> innerKeySelector,
                Expression<Func<T, TInner, TResult>> resultSelector);

            IRemote<TResult> GroupJoin<TInner, TKey, TResult>(
                IRemote<TInner> inner,
                Expression<Func<T, TKey>> outerKeySelector,
                Expression<Func<TInner, TKey>> innerKeySelector,
                Expression<Func<T, IRemote<TInner>, TResult>> resultSelector);
        }

        public interface IOrderedRemote<T> : IRemote<T>
        {
            IOrderedRemote<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelector);

            IOrderedRemote<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        }

        public interface IRemoteGroup<TKey, T>
        {
            TKey Key { get; }
        }

        internal static void QueryExpressionAndQueryMethod(IEnumerable<int> source)
        {
            IEnumerable<int> query =
                (from int32 in source
                 where int32 > 0
                 select int32)
                .Skip(20)
                .Take(10);
        }

        internal static void WhereWithIndex(IEnumerable<int> source)
        {
            IEnumerable<int> query = source.Where((int32, index) => int32 > 0 && index % 2 == 0);
        }
    }

    internal static partial class CompositionsWithLinq
    {
        private static TResult Select<TSource, TResult>(
            this TSource source, Func<TSource, TResult> selector) =>
            selector(source);

        internal static void ForwardPipingWithSelect()
        {
            double squareRoot;
            squareRoot = from @string in "-2"
                         select int.Parse(@string) into int32
                         select Math.Abs(int32) into absolute
                         select Convert.ToDouble(absolute) into @double
                         select Math.Sqrt(@double);
            squareRoot = "-2"
                .Select(int.Parse)
                .Select(Math.Abs)
                .Select(Convert.ToDouble)
                .Select(Math.Sqrt);
        }

        private static TResult SelectMany<TSource, TSelector, TResult>(
            this TSource source,
            Func<TSource, TSelector> selector,
            Func<TSource, TSelector, TResult> resultSelector)
        {
            TSelector selectorResult = selector(source);
            return resultSelector(source, selectorResult);
        }

        internal static void ForwardPipingWithQueryExpression()
        {
            double result;
            result = from @string in "-2"
                     from int32 in int.Parse(@string)
                     from absolute in Math.Abs(int32)
                     from @double in Convert.ToDouble(absolute)
                     from squareRoot in Math.Sqrt(@double)
                     select squareRoot;
            result = "-2"
                .SelectMany(
                    @string => int.Parse(@string),
                    (@string, int32) => new { @string, int32 })
                .SelectMany(
                    context => Math.Abs(context.int32),
                    (context, absolute) => new { context, absolute })
                .SelectMany(
                    context => Convert.ToDouble(context.absolute),
                    (context, @double) => new { context, @double })
                .SelectMany(
                    context => Math.Sqrt(context.@double),
                    (context, squareRoot) => squareRoot);
        }
    }
}

#if DEMO
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

        // Other members.
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector);

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IOrderedEnumerable<out TElement> : IEnumerable<TElement>, IEnumerable
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }

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

    public interface IOrderedQueryable<out T> : IEnumerable<T>, IEnumerable, IOrderedQueryable, IQueryable, IQueryable<T>
    {
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

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TResult> Empty<TResult>();

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count);
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static TSource First<TSource>(this IEnumerable<TSource> source);

        public static int Count<TSource>(this IEnumerable<TSource> source);
    }

    public static class Queryable
    {
        public static TSource First<TSource>(this IQueryable<TSource> source);

        public static int Count<TSource>(this IQueryable<TSource> source);
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count);

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count);
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate);
    }
}
#endif