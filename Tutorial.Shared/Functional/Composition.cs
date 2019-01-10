namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;

    internal static partial class Compositions
    {
        internal static void OutputAsInput<T, TResult1, TResult2>(
            Func<TResult1, TResult2> second, // TResult1 -> TResult2
            Func<T, TResult1> first) // T -> TResult1
        {
            Func<T, TResult2> composition = value => second(first(value)); // T -> TResult2
        }

        internal static void OutputAsInput()
        {
            string @string = "-2";
            int int32 = int.Parse(@string); // string -> int
            int absolute = Math.Abs(int32); // int -> int
            double @double = Convert.ToDouble(absolute); // int -> double
            double squareRoot = Math.Sqrt(@double); // double -> double
        }

        // string -> double
        internal static double Composition(string value) =>
            Math.Sqrt(Convert.ToDouble(Math.Abs(int.Parse(value))));

        // Input: TResult1 -> TResult2, T -> TResult1.
        // Output: T -> TResult2
        public static Func<T, TResult2> After<T, TResult1, TResult2>(
            this Func<TResult1, TResult2> second, Func<T, TResult1> first) =>
            value => second(first(value));

        // Input: T -> TResult1, TResult1 -> TResult2.
        // Output: T -> TResult2
        public static Func<T, TResult2> Then<T, TResult1, TResult2>(
            this Func<T, TResult1> first, Func<TResult1, TResult2> second) =>
            value => second(first(value));

        internal static void Composition<T, TResult1, TResult2>(
            Func<TResult1, TResult2> second, // TResult1 -> TResult2
            Func<T, TResult1> first) // T -> TResult1
        {
            Func<T, TResult2> composition; // T -> TResult2
            composition = second.After(first);
            // Equivalent to:
            composition = first.Then(second);
        }

        internal static void BackwardCompositionForwardComposition()
        {
            Func<string, int> parse = int.Parse; // string -> int
            Func<int, int> abs = Math.Abs; // int -> int
            Func<int, double> convert = Convert.ToDouble; // int -> double
            Func<double, double> sqrt = Math.Sqrt; // double -> double

            // string -> double
            Func<string, double> backwardComposition = sqrt.After(convert).After(abs).After(parse);
            backwardComposition("-2").WriteLine(); // 1.4142135623731

            // string -> double
            Func<string, double> forwardComposition = parse.Then(abs).Then(convert).Then(sqrt);
            forwardComposition("-2").WriteLine(); // 1.4142135623731
        }

        internal static void ListFunctions()
        {
            List<int> list = new List<int>() { -2, -1, 0, 1, 2 };
            list.RemoveAt(0); // -> void
            list = list.FindAll(int32 => int32 > 0); // -> List<T>
            list.Reverse(); // -> void
            list.ForEach(int32 => int32.WriteLine()); // -> void
            list.Clear(); // -> void
            list.Add(1); // -> void
        }

        // T -> List<T> -> List<T>
        internal static Func<List<T>, List<T>> Add<T>(T value) =>
            list =>
                {
                    list.Add(value);
                    return list;
                };

        // List<T> -> List<T>
        internal static List<T> Clear<T>(List<T> list)
        {
            list.Clear();
            return list;
        }

        // Predicate<T> -> List<T> -> List<T>
        internal static Func<List<T>, List<T>> FindAll<T>(Predicate<T> match) =>
            list => list.FindAll(match);

        // Action<T> -> List<T> -> List<T>
        internal static Func<List<T>, List<T>> ForEach<T>(Action<T> action) =>
            list =>
                {
                    list.ForEach(action);
                    return list;
                };

        // int -> List<T> -> List<T>
        internal static Func<List<T>, List<T>> RemoveAt<T>(int index) =>
            list =>
                {
                    list.RemoveAt(index);
                    return list;
                };

        // List<T> -> List<T>
        internal static List<T> Reverse<T>(List<T> list)
        {
            list.Reverse();
            return list;
        }

        internal static void TransformationForComposition()
        {
            // List<int> -> List<int>
            Func<List<int>, List<int>> removeAtWithIndex = RemoveAt<int>(0);
            Func<List<int>, List<int>> findAllWithPredicate = FindAll<int>(int32 => int32 > 0);
            Func<List<int>, List<int>> reverse = Reverse;
            Func<List<int>, List<int>> forEachWithAction = ForEach<int>(int32 => int32.WriteLine());
            Func<List<int>, List<int>> clear = Clear;
            Func<List<int>, List<int>> addWithValue = Add(1);

            Func<List<int>, List<int>> backwardComposition =
                addWithValue
                    .After(clear)
                    .After(forEachWithAction)
                    .After(reverse)
                    .After(findAllWithPredicate)
                    .After(removeAtWithIndex);

            Func<List<int>, List<int>> forwardComposition =
                removeAtWithIndex
                    .Then(findAllWithPredicate)
                    .Then(reverse)
                    .Then(forEachWithAction)
                    .Then(clear)
                    .Then(addWithValue);
        }

        internal static void ForwardComposition()
        {
            Func<List<int>, List<int>> forwardComposition =
                RemoveAt<int>(0)
                    .Then(FindAll<int>(int32 => int32 > 0))
                    .Then(Reverse)
                    .Then(ForEach<int>(int32 => int32.WriteLine()))
                    .Then(Clear)
                    .Then(Add(1));

            List<int> list = new List<int>() { -2, -1, 0, 1, 2 };
            List<int> result = forwardComposition(list);
        }

        // Input, T, T -> TResult.
        // Output TResult.
        public static TResult Forward<T, TResult>(this T value, Func<T, TResult> function) =>
            function(value);

        internal static void OutputAsInput<T, TResult1, TResult2>(
            Func<TResult1, TResult2> second, // TResult1 -> TResult2
            Func<T, TResult1> first, // T -> TResult1
            T value)
        {
            TResult2 result = value.Forward(first).Forward(second);
        }

        internal static void ForwardPiping()
        {
            double result = "-2"
                .Forward(int.Parse) // string -> int
                .Forward(Math.Abs) // int -> int
                .Forward(Convert.ToDouble) // int -> double
                .Forward(Math.Sqrt); // double -> double
        }

        internal static void ForwardPipingWithPartialApplication()
        {
            List<int> result = new List<int>() { -2, -1, 0, 1, 2 }
                .Forward(RemoveAt<int>(1))
                .Forward(FindAll<int>(int32 => int32 > 0))
                .Forward(Reverse)
                .Forward(ForEach<int>(int32 => int32.WriteLine()))
                .Forward(Clear)
                .Forward(Add(1));
        }

        internal static void InstanceMethodChaining(string @string)
        {
            string result = @string.Trim().Substring(1).Remove(10).ToUpperInvariant();
        }

        internal class FluentList<T>
        {
            internal FluentList(List<T> list) => this.List = list;

            internal List<T> List { get; }

            internal FluentList<T> Add(T value)
            {
                this.List.Add(value);
                return this;
            }

            internal FluentList<T> Clear()
            {
                this.List.Clear();
                return this;
            }

            internal FluentList<T> FindAll(Predicate<T> predicate) => new FluentList<T>(this.List.FindAll(predicate));

            internal FluentList<T> ForEach(Action<T> action)
            {
                this.List.ForEach(action);
                return this;
            }

            internal FluentList<T> RemoveAt(int index)
            {
                this.List.RemoveAt(index);
                return this;
            }

            internal FluentList<T> Reverse()
            {
                this.List.Reverse();
                return this;
            }
        }

        internal static void InstanceMethodChaining()
        {
            List<int> list = new List<int>() { -2, -1, 0, 1, 2 };
            FluentList<int> resultWrapper = new FluentList<int>(list)
                .RemoveAt(0)
                .FindAll(int32 => int32 > 0)
                .Reverse()
                .ForEach(int32 => int32.WriteLine())
                .Clear()
                .Add(1);
            List<int> result = resultWrapper.List;
        }

        internal static List<T> ExtensionAdd<T>(this List<T> list, T item)
        {
            list.Add(item);
            return list;
        }

        internal static List<T> ExtensionClear<T>(this List<T> list)
        {
            list.Clear();
            return list;
        }

        internal static List<T> ExtensionFindAll<T>(this List<T> list, Predicate<T> predicate) =>
            list.FindAll(predicate);

        internal static List<T> ExtensionForEach<T>(this List<T> list, Action<T> action)
        {
            list.ForEach(action);
            return list;
        }

        internal static List<T> ExtensionRemoveAt<T>(this List<T> list, int index)
        {
            list.RemoveAt(index);
            return list;
        }

        internal static List<T> ExtensionReverse<T>(this List<T> list)
        {
            list.Reverse();
            return list;
        }

        internal static void ExtensionMethodComposition()
        {
            List<int> result = new List<int>() { -2, -1, 0, 1, 2 }
                .ExtensionRemoveAt(0)
                .ExtensionFindAll(int32 => int32 > 0)
                .ExtensionReverse()
                .ExtensionForEach(int32 => int32.WriteLine())
                .ExtensionClear()
                .ExtensionAdd(1);
        }

        internal static void CompiledExtensionMethodComposition()
        {
            List<int> result =
                ExtensionAdd(
                    ExtensionClear(
                        ExtensionForEach(
                            ExtensionReverse(
                                ExtensionFindAll(
                                    ExtensionRemoveAt(
                                        new List<int>() { -2, -1, 0, 1, 2 },
                                        0
                                    ),
                                    int32 => int32 > 0
                                )
                            ),
                            int32 => int32.WriteLine()
                        )
                    ),
                    1
                );
        }

        internal static int ParseInt32(this string @string) => int.Parse(@string);

        internal static int Abs(this int int32) => Math.Abs(int32);

        internal static double ToDouble(this int int32) => Convert.ToDouble(int32);

        internal static double Sqrt(this double @double) => Math.Sqrt(@double);

        internal static void ForwardPipingWithExtensionMethod()
        {
            double result = "-2"
                .ParseInt32() // .Forward(int.Parse)
                .Abs() // .Forward(Math.Abs)
                .ToDouble() // .Forward(Convert.ToDouble)
                .Sqrt(); // .Forward(Math.Sqrt);
        }
    }

    internal interface IAnimal
    {
        IAnimal Eat();

        IAnimal Move();
    }

    internal static class AnimalExtensions
    {
        internal static IAnimal Sleep(this IAnimal animal) => animal;
    }

    internal static partial class Compositions
    {
        internal static void FluentInterface(IAnimal animal)
        {
            IAnimal result = animal.Eat().Move().Sleep();
        }
    }
}

#if DEMO
namespace System.Collections.Generic
{
    public class List<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        public void Add(T item); // (List<T>, T) -> void

        public void Clear(); // List<T> -> void

        public List<T> FindAll(Predicate<T> match); // (List<T>, Predicate<T>) -> List<T>

        public void ForEach(Action<T> action); // (List<T>, Action<T>) -> void

        public void RemoveAt(int index); // (List<T>, index) -> void

        public void Reverse(); // List<T> -> void

        // Other members.
    }
}
#endif
