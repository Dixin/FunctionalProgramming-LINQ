namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;

    // () -> void.
    internal delegate void Function();

    internal static partial class FirstClassFunctions
    {
        internal partial class Data
        {
            internal Data(int value) => this.Value = value;

            internal int Value { get; }
        }

        internal static void CallFirstOrderFunction()
        {
            Data FirstOrderFunction(Data value) { return value; }

            Data input = default;
            Data output = FirstOrderFunction(input);
        }

        internal static void CallNamedHigherOrderFunction()
        {
            Function NamedHigherOrderFunction(Function value) { return value; }

            Function input = default;
            Function output = NamedHigherOrderFunction(input);
        }

        internal static void CallAnonymousHigherOrderFunction()
        {
            Action firstOrder1 = () => nameof(firstOrder1).WriteLine();
            Func<int> firstOrder2 = () => 1;

            // (() -> void) -> void
            // Input: function of type () -> void. Output: void.
            Action<Action> higherOrder1 = action => action();
            higherOrder1(firstOrder1); // firstOrder1
            higherOrder1(() => nameof(higherOrder1).WriteLine()); // higherOrder1

            // () -> (() -> int)
            // Input: none. Output: function of type () -> int.
            Func<Func<int>> higherOrder2 = () => firstOrder2;
            Func<int> output2 = higherOrder2();
            output2().WriteLine(); // 1

            // int -> (() -> int)
            // Input: value of type int. Output: function of type () -> int.
            Func<int, Func<int>> higherOrder3 = int32 => (() => int32 + 1);
            Func<int> output3 = higherOrder3(1);
            output3().WriteLine(); // 2

            // (() -> void, () -> int) -> (() -> bool)
            // Input: function of type () -> void, function of type () -> int. Output: function of type () -> bool.
            Func<Action, Func<int>, Func<bool>> higherOrder4 = (action, int32Factory) =>
            {
                action();
                return () => int32Factory() > 0;
            };
            Func<bool> output4 = higherOrder4(firstOrder1, firstOrder2); // firstOrder1
            output4().WriteLine(); // True
            Func<bool> output5 = higherOrder4(() => nameof(higherOrder4).WriteLine(), () => 0); // higherOrder4
            output5().WriteLine(); // False
        }

        internal static void AnonymousHigherOrderIife()
        {
            // (() -> void) -> void
            new Action<Action>(action => action())(
                () => nameof(AnonymousHigherOrderIife).WriteLine()); // AnonymousHigherOrderIife

            // () -> (() -> int)
            Func<int> output2 = new Func<Func<int>>(() => (() => 1))();
            output2().WriteLine(); // 1

            // int -> (() -> int)
            Func<int> output3 = new Func<int, Func<int>>(int32 => (() => int32 + 1))(1);
            output3().WriteLine(); // 2

            // (() -> int, () -> string) -> (() -> bool)
            Func<bool> output4 = new Func<Action, Func<int>, Func<bool>>((action, int32Factory) =>
            {
                action();
                return () => int32Factory() > 0;
            })(arg1: () => nameof(AnonymousHigherOrderIife).WriteLine(), arg2: () => 0); // AnonymousHigherOrderIife
            output4().WriteLine();
        }

        internal static void FilterArray(Data[] array)
        {
            Data[] filtered = Array.FindAll(array, data => data != null);
        }
        internal static void Add2ArgsFirstOrderToHigherOrder()
        {
            // (int, int) -> int
            Func<int, int, int> add2Args = (a, b) => a + b;
            int result = add2Args(1, 2); // 3

            // int -> (int -> int)
            // Input: value of type int. output: function of type int -> int.
            Func<int, Func<int, int>> higherOrderAdd2Args = a =>
                new Func<int, int>(b => a + b);
            Func<int, int> add1ArgAnd1Constant = higherOrderAdd2Args(1); // Equivalent to: b => 1 + b.
            int higherOrderResult = add1ArgAnd1Constant(2); // 3
            // Equivalent to: int higherOrderResult = higherOrderAdd2Args(1)(2);
        }

        internal static void Add3ArgsFirstOrderToHigherOrder()
        {
            // (int, int, int) -> int
            Func<int, int, int, int> add3Args = (a, b, c) => a + b + c;
            int result = add3Args(1, 2, 3); // 6

            // int -> (int -> (int -> int))
            // Input: value of type int. output: function of type int -> (int -> int), the same as above higherOrderSumOfTwoIntegers.
            Func<int, Func<int, Func<int, int>>> higherOrderAdd3Args = a =>
                new Func<int, Func<int, int>>(b =>
                    new Func<int, int>(c => a + b + c));
            Func<int, Func<int, int>> higherOrderAdd2ArgsAnd1Constant = higherOrderAdd3Args(1); // Equivalent to: b => (c => 1 + b + c).
            Func<int, int> add1ArgAnd2Constants = higherOrderAdd2ArgsAnd1Constant(2); // Equivalent to: c => 1 + 2 + c.
            int higherOrderResult = add1ArgAnd2Constants(3); // 6
            // Equivalent to: int higherOrderResult = higherOrderAdd3Args(1)(2)(3);
        }

        internal static void TypeInference()
        {
            // (int, int) -> int
            Func<int, int, int> add2Args = (a, b) => a + b;

            // int -> (int -> int)
            Func<int, Func<int, int>> higherOrderAdd2Args = a => (b => a + b);

            // (int, int, int) -> int
            Func<int, int, int, int> add3Args = (a, b, c) => a + b + c;

            // int -> (int -> (int -> int))
            Func<int, Func<int, Func<int, int>>> higherOrderAdd3Args = a => (b => (c => a + b + c));
        }

        internal static void LambdaOperatorAssociativity()
        {
            // (int, int) -> int
            Func<int, int, int> add2Args = (a, b) => a + b;

            // int -> int -> int
            Func<int, Func<int, int>> higherOrderAdd2Args = a => b => a + b;

            // (int, int, int) -> int
            Func<int, int, int, int> add3Args = (a, b, c) => a + b + c;

            // int -> int -> int -> int
            Func<int, Func<int, Func<int, int>>> higherOrderAdd3Args = a => b => c => a + b + c;
        }

        internal static void Object()
        {
            Data value = new Data(0);
            ref Data alias = ref value;
            ref readonly Data immutableAlias = ref value;
        }

        internal static void Function()
        {
            Function value1 = Function; // Named function.
            Function value2 = () => { }; // Anonymous function.
            ref Function alias = ref value1;
            ref readonly Function immutableAlias = ref value2;
        }

        internal class Fields
        {
            private static Data staticDataField = new Data(0);

            private static Function staticNamedFunctionField = Function;

            private static Function staticAnonymousFunctionField = () => { };

            private Data instanceDataField = new Data(0);

            private Function instanceNamedFunctionField = Function;

            private Function instanceAnonymousFunctionField = () => { };
        }

        internal partial class Data
        {
            internal Data Inner { get; set; }
        }

        internal static void NestedObject()
        {
            Data outer = new Data(1)
            {
                Inner = new Data(2)
            };
        }

        internal static void NestedFunction()
        {
            void Outer()
            {
                void Inner() { }
            }

            Function outer = () =>
            {
                Function inner = () => { };
            };
        }

        internal class OuterClass
        {
            const int Outer = 1;

            class InnerClass
            {
                const int Inner = 2;
                int sum = Inner + Outer;
            }
        }

        internal static void OuterFunction()
        {
            const int Outer = 1;

            void InnerFunction()
            {
                const int Inner = 2;
                int sum = Inner + Outer;
            }

            new Function(() =>
            {
                const int Inner = 2;
                int sum = Inner + Outer;
            })();
        }

        internal static Data Function(Data value) => value;

        internal static Function Function(Function value) => value;

        internal partial class Data
        {
            public override bool Equals(object obj) => 
                object.ReferenceEquals(this, obj) || this.Value == (obj as Data)?.Value;

            public override int GetHashCode() => this.Value.GetHashCode();

            public static bool operator ==(Data data1, Data data2) => data1?.Value == data2?.Value;

            public static bool operator !=(Data data1, Data data2) => !(data1 == data2);
        }

        internal static void ObjectEquality()
        {
            Data value1 = new Data(1);
            Data value2 = new Data(1);
            object.ReferenceEquals(value1, value2).WriteLine(); // False
            object.Equals(value1, value2).WriteLine(); // True
            value1.Equals(value2).WriteLine(); // True
            (value1 == value2).WriteLine(); // True
            (value1.GetHashCode() == value2.GetHashCode()).WriteLine(); // True.
            EqualityComparer<Data>.Default.Equals(value1, value2).WriteLine(); // True
        }

        internal static void FunctionEquality()
        {
            Function value1 = Function;
            Function value2 = Function;
            object.ReferenceEquals(value1, value2).WriteLine(); // False
            object.Equals(value1, value2).WriteLine(); // True
            value1.Equals(value2).WriteLine(); // True
            (value1 == value2).WriteLine(); // True
            (value1.GetHashCode() == value2.GetHashCode()).WriteLine(); // True.
            EqualityComparer<Function>.Default.Equals(value1, value2).WriteLine(); // True
        }
    }
}

#if DEMO
namespace System
{
    using System.Collections;

    public abstract class Array : ICollection, IEnumerable, IList, IStructuralComparable, IStructuralEquatable
    {
        public static T[] FindAll<T>(T[] array, Predicate<T> match);
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Where<TSource>(
            this IEnumerable<TSource> source, Func<TSource, bool> predicate);

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);
    }

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Func<TSource, bool> predicate);

        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
            this IQueryable<TSource> source, Func<TSource, TKey> keySelector);

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Func<TSource, TResult> selector);
    }
}
#endif