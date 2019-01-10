namespace Tutorial.Functional
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal interface ISupertype { /* Members. */ }

    internal interface ISubtype : ISupertype { /* More members. */ }

    internal class Base { }

    internal class Derived : Base { }

    internal static partial class Variances
    {
        internal static void Substitute(ISupertype supertype, ISubtype subtype)
        {
            supertype = subtype;
        }

        internal static void Substitute()
        {
            Base @base = new Base();
            @base = new Derived();
        }

        // Derived -> Base
        internal static Base DerivedToBase(Derived input) => new Base();

        // Derived -> Derived
        internal static Derived DerivedToDerived(Derived input) => new Derived();

        // Base -> Base
        internal static Base BaseToBase(Base input) => new Base();

        // Base -> Derived
        internal static Derived BaseToDerived(Base input) => new Derived();
    }

    // Derived -> Base
    internal delegate Base DerivedToBase(Derived input);

    // Derived -> Derived
    internal delegate Derived DerivedToDerived(Derived input);

    // Base -> Base
    internal delegate Base BaseToBase(Base input);

    // Base -> Derived
    internal delegate Derived BaseToDerived(Base input);

    internal static partial class Variances
    {
        internal static void NonGenericDelegate()
        {
            DerivedToDerived derivedToDerived = DerivedToDerived;
            Derived output = derivedToDerived(input: new Derived());
        }

        internal static void NonGenericDelegateCovariance()
        {
            DerivedToBase derivedToBase = DerivedToBase; // Derived -> Base

            // Covariance: Derived <: Base, so that Derived -> Derived <: Derived -> Base.
            derivedToBase = DerivedToDerived; // Derived -> Derived

            // When calling derivedToBase, DerivedToDerived executes.
            // derivedToBase should output Base, while DerivedToDerived outputs Derived.
            // The actual Derived output substitutes the required Base output. This call always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericDelegateContravariance()
        {
            DerivedToBase derivedToBase = DerivedToBase; // Derived -> Base

            // Contravariance: Derived <: Base, so that Base -> Base <: Derived -> Base.
            derivedToBase = BaseToBase; // Base -> Base

            // When calling derivedToBase, BaseToBase executes.
            // derivedToBase should accept Derived input, while BaseToBase accepts Base input.
            // The required Derived input substitutes the accepted Base input. This always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericDelegateCovarianceAndContravariance()
        {
            DerivedToBase derivedToBase = DerivedToBase; // Derived -> Base

            // Covariance and contravariance: Derived <: Base, so that Base -> Derived <: Derived -> Base.
            derivedToBase = BaseToDerived; // Base -> Derived

            // When calling derivedToBase, BaseToDerived executes.
            // derivedToBase should accept Derived input, while BaseToDerived accepts Base input.
            // The required Derived input substitutes the accepted Base input.
            // derivedToBase should output Base, while BaseToDerived outputs Derived.
            // The actual Derived output substitutes the required Base output. This always works.
            Base output = derivedToBase(input: new Derived());
        }

        internal static void NonGenericDelegateInvariance()
        {
#if DEMO
            // baseToDerived should output Derived, while BaseToBase outputs Base. 
            // The actual Base output does not substitute the required Derived output. This cannot be compiled.
            BaseToDerived baseToDerived = BaseToBase; // Base -> Derived

            // baseToDerived should accept Base input, while DerivedToDerived accepts Derived input.
            // The required Base input does not substitute the accepted Derived input. This cannot be compiled.
            baseToDerived = DerivedToDerived; // Derived -> Derived

            // baseToDerived should accept Base input, while DerivedToBase accepts Derived input.
            // The required Base input does not substitute the expected Derived input.
            // baseToDerived should output Derived, while DerivedToBase outputs Base.
            // The actual Base output does not substitute the required Derived output. This cannot be compiled.
            baseToDerived = DerivedToBase; // Derived -> Base
#endif
        }

        internal delegate TOutput GenericFunc<TInput, TOutput>(TInput input);

        internal static void GenericDelegate()
        {
            GenericFunc<Derived, Base> derivedToBase = DerivedToBase; // No variance.
            derivedToBase = DerivedToDerived; // Covariance.
            derivedToBase = BaseToBase; // Contravariance.
            derivedToBase = BaseToDerived; // Covariance and contravariance.
        }

        internal delegate TOutput GenericFuncWithVariances<in TInput, out TOutput>(TInput input);

        internal static void GenericDelegateCovarianceAndContravariance()
        {
            GenericFuncWithVariances<Derived, Base> derivedToBase = DerivedToBase; // Derived -> Base: no variances.
            derivedToBase = DerivedToDerived; // Derived -> Derived: covariance.
            derivedToBase = BaseToBase; // Base -> Base: contravariance.
            derivedToBase = BaseToDerived; // Base -> Derived: covariance and contravariance.
        }

        internal static void GenericDelegateInstanceSubstitution()
        {
            GenericFuncWithVariances<Derived, Base> derivedToBase = DerivedToBase; // Derived -> Base
            GenericFuncWithVariances<Derived, Derived> derivedToDerived = DerivedToDerived; // Derived -> Derived
            GenericFuncWithVariances<Base, Base> baseToBase = BaseToBase; // Base -> Base
            GenericFuncWithVariances<Base, Derived> baseToDerived = BaseToDerived; // Base -> Derived

            // Cannot be compiled without the out/in modifiers.
            derivedToBase = derivedToDerived; // Covariance.
            derivedToBase = baseToBase; // Contravariance.
            derivedToBase = baseToDerived; // Covariance and contravariance.
        }

#if DEMO
    // Cannot be compiled.
    internal delegate TOutput GenericFuncWithVariances<out TInput, in TOutput>(TInput input);
#endif

        internal interface IOutput<out TOutput> // TOutput is covariant for all members using TOutput.
        {
            TOutput ToOutput(); // TOutput is covariant.

            TOutput Output { get; } // Compiled to get_Output. TOutput is covariant.

            void TypeParameterNotUsed();
        }

        internal static void GenericInterfaceCovariance(
            IOutput<Base> outputBase, IOutput<Derived> outputDerived)
        {
            // Covariance: Derived <: Base, so that IOutput<Derived> <: IOutput<Base>.
            outputBase = outputDerived;

            // When calling outputBase.ToOutput, outputDerived.ToOutput executes.
            // outputBase.ToOutput should output Base, outputDerived.ToOutput outputs Derived.
            // The actual Derived output substitutes the required Base output. This always works.
            Base output1 = outputBase.ToOutput();

            Base output2 = outputBase.Output; // .get_Output();
        }

        internal interface IInput<in TInput> // TInput is contravariant for all members using TInput.
        {
            void InputToVoid(TInput input); // TInput is contravariant.

            TInput Input { set; } // Compiled to set_Input. TInput is contravariant.

            void TypeParameterNotUsed();
        }

        internal static void GenericInterfaceContravariance(
            IInput<Derived> inputDerived, IInput<Base> inputBase)
        {
            // Contravariance: Derived <: Base, so that IInput<Base> <: IInput<Derived>.
            inputDerived = inputBase;

            // When calling inputDerived.Input, inputBase.Input executes.
            // inputDerived.Input should accept Derived input, while inputBase.Input accepts Base input.
            // The required Derived output substitutes the accepted Base input. This always works.
            inputDerived.InputToVoid(input: new Derived());

            inputDerived.Input = new Derived(); // .set_Input(input: new Derived());
        }

        internal interface IInputOutput<in TInput, out TOutput> // TInput is contravariant for all members using TInput, TOutput is covariant for all members usingTOutput.
        {
            void InputToVoid(TInput input); // TInput is contravariant.

            TInput Input { set; } // Compiled to set_Input. TInput is contravariant.

            TOutput ToOutput(); // TOutput is covariant.

            TOutput Output { get; } // Compiled to get_Output. TOutput is covariant.

            void TypeParameterNotUsed();
        }

        internal static void GenericInterfaceCovarianceAndContravariance(
            IInputOutput<Derived, Base> inputDerivedOutputBase,
            IInputOutput<Base, Derived> inputBaseOutputDerived)
        {
            // Covariance and contravariance: Derived <: Base, so that IInputOutput<Base, Derived> <: IInputOutput<Derived, Base>.
            inputDerivedOutputBase = inputBaseOutputDerived;

            inputDerivedOutputBase.InputToVoid(new Derived());
            inputDerivedOutputBase.Input = new Derived(); // .set_Input(input: new Derived());
            Base output1 = inputDerivedOutputBase.ToOutput();
            Base output2 = inputDerivedOutputBase.Output; // .get_Output();
        }

        internal interface IInvariant<T>
        {
            T Output(); // T is covariant for Output: () -> T.

            void Input(T input); // T is contravariant for Input: T -> void.
        }

        internal static void OutputCovariance()
        {
            // First order functions.
            Func<Base> toBase = () => new Base(); // () -> Base
            Func<Derived> toDerived = () => new Derived(); // () -> Derived

            // Higher-order functions.
            ToFunc<Base> toToBase = () => toBase; // () -> () -> Base
            ToFunc<Derived> toToDerived = () => toDerived; // () -> () -> Derived

            // Covariance: Derived <: Base, so that () -> () -> Derived <: () -> () -> Base.
            toToBase = toToDerived;

            // When calling toToBase, toToDerived executes.
            // toToBase should output Func<Base>, while toToDerived outputs Func<Derived>.
            // The actual Func<Derived> output substitutes the required Func<Base> output. This always works.
            Func<Base> output = toToBase();
        }

        // () -> TOutput
        internal delegate TOutput Func<out TOutput>(); // Covariant.

        // () -> () -> TOutput: Equivalent to Func<Func<TOutput>>.
        internal delegate Func<TOutput> ToFunc<out TOutput>(); // Covariant.

        // () -> () -> () -> TOutput: Equivalent to Func<Func<Func<TOutput>>>.
        internal delegate ToFunc<TOutput> ToToFunc<out TOutput>(); // Covariant.

        // () -> () -> () -> () -> TOutput: Equivalent to Func<Func<Func<Func<TOutput>>>>.
        internal delegate ToToFunc<TOutput> ToToToFunc<out TOutput>(); // Covariant.

        // ...

#if DEMO
        // (TInput -> void) -> void: Equivalent to Action<Action<TInput>>.
        internal delegate void ActionToVoid<out TInput>(Action<TInput> action);
#endif

        internal static void InputCovarianceAndContravariance()
        {
            // Higher-order functions.
            ActionToVoid<Derived> derivedToVoidToVoid = (Action<Derived> derivedToVoid) => { };
            ActionToVoid<Base> baseToVoidToVoid = (Action<Base> baseToVoid) => { };

            // Covariance: Derived <: Base, so that(Derived -> void) -> void <: (Base -> void) -> void.
            baseToVoidToVoid = derivedToVoidToVoid;

            // When calling baseToVoidToVoid, derivedToVoidToVoid executes.
            // baseToVoidToVoid should accept Action<Base> input, while derivedToVoidToVoid accepts Action<Derived> input.
            // The required Action<Derived> input substitutes the accepted Action<Base> input. This always works.
            baseToVoidToVoid(default(Action<Base>));
        }

        // TInput -> void
        internal delegate void Action<in TInput>(TInput input); // Contravariant.

        // (TInput -> void) -> void: Equivalent to Action<Action<TInput>>.
        internal delegate void ActionToVoid<out TTInput>(Action<TTInput> action); // Covariant.

        // ((TInput -> void) -> void) -> void: Equivalent to Action<Action<Action<TInput>>>.
        internal delegate void ActionToVoidToVoid<in TTInput>(ActionToVoid<TTInput> actionToVoid); // Contravariant.

        // (((TInput  -> void) -> void) -> void) -> void: Equivalent to Action<Action<Action<Action<TInput>>>>.
        internal delegate void ActionToVoidToVoidToVoid<out TTInput>(ActionToVoidToVoid<TTInput> actionToVoidToVoid); // Covariant.

        // ...

        internal static void ArrayCovariance()
        {
            Base[] baseArray = new Base[3];
            Derived[] derivedArray = new Derived[3];

            baseArray = derivedArray; // Array covariance: Derived <: Base, so that Derived[] <: Base[], baseArray refers to a Derived array at runtime.
            baseArray[1] = new Derived(); // .set_Item(new Derived());
            baseArray[2] = new Base(); // .set_Item(new Base());
            // ArrayTypeMismatchException at runtime. The actual Derived array requires Derived instance, the provided Base instance cannot substitute Derived instance.
        }

        internal static void TypesWithVariance()
        {
            Assembly coreLibrary = typeof(object).Assembly;
            coreLibrary.ExportedTypes
                .Where(type => type.GetGenericArguments().Any(typeArgument =>
                {
                    GenericParameterAttributes attributes = typeArgument.GenericParameterAttributes;
                    return attributes.HasFlag(GenericParameterAttributes.Covariant)
                        || attributes.HasFlag(GenericParameterAttributes.Contravariant);
                }))
                .OrderBy(type => type.FullName)
                .WriteLines();
            // System.Action`1[T]
            // System.Action`2[T1,T2]
            // System.Action`3[T1,T2,T3]
            // System.Action`4[T1,T2,T3,T4]
            // System.Action`5[T1,T2,T3,T4,T5]
            // System.Action`6[T1,T2,T3,T4,T5,T6]
            // System.Action`7[T1,T2,T3,T4,T5,T6,T7]
            // System.Action`8[T1,T2,T3,T4,T5,T6,T7,T8]
            // System.Collections.Generic.IComparer`1[T]
            // System.Collections.Generic.IEnumerable`1[T]
            // System.Collections.Generic.IEnumerator`1[T]
            // System.Collections.Generic.IEqualityComparer`1[T]
            // System.Collections.Generic.IReadOnlyCollection`1[T]
            // System.Collections.Generic.IReadOnlyList`1[T]
            // System.Comparison`1[T]
            // System.Converter`2[TInput,TOutput]
            // System.Func`1[TResult]
            // System.Func`2[T,TResult]
            // System.Func`3[T1,T2,TResult]
            // System.Func`4[T1,T2,T3,TResult]
            // System.Func`5[T1,T2,T3,T4,TResult]
            // System.Func`6[T1,T2,T3,T4,T5,TResult]
            // System.Func`7[T1,T2,T3,T4,T5,T6,TResult]
            // System.Func`8[T1,T2,T3,T4,T5,T6,T7,TResult]
            // System.Func`9[T1,T2,T3,T4,T5,T6,T7,T8,TResult]
            // System.IComparable`1[T]
            // System.IObservable`1[T]
            // System.IObserver`1[T]
            // System.IProgress`1[T]
            // System.Predicate`1[T]
        }

        internal static void Concat(
            IEnumerable<Base> enumerableOfBase, IEnumerable<Derived> enumerableOfDerived)
        {
            // Covariance of Concat input: IEnumerable<Derived> <: IEnumerable<Base>.
            // Concat: (IEnumerable<Base>, IEnumerable<Base>) -> IEnumerable<Base>.
            enumerableOfBase = enumerableOfBase.Concat(enumerableOfDerived);
        }

        internal static void Select(IEnumerable<Derived> enumerableOfDerived)
        {
            IEnumerable<Base> enumerableOfBase;
            // Default with no variance.
            // Select: (IEnumerable<Derived>, Derived -> Base) -> IEnumerable<Base>.
            enumerableOfBase = enumerableOfDerived.Select(DerivedToBase);

            // Covariance of Select input: IEnumerable<Derived> <: IEnumerable<Base>.
            // Select: (IEnumerable<Base>, Base -> Base) -> IEnumerable<Base>.
            enumerableOfBase = enumerableOfDerived.Select(BaseToBase);

            // Covariance of Select output: IEnumerable<Derived> <: IEnumerable<Base>.
            // Select: (IEnumerable<Derived>, Derived -> Derived) -> IEnumerable<Derived>.
            enumerableOfBase = enumerableOfDerived.Select(DerivedToDerived);

            // Covariance of Select input and output: IEnumerable<Derived> <: IEnumerable<Base>.
            // Select: (IEnumerable<Base>, Base -> Derived) -> IEnumerable<Derived>.
            enumerableOfBase = enumerableOfDerived.Select(BaseToDerived);
        }
    }
}

#if DEMO
namespace System
{
    public delegate TResult Func<out TResult>();

    public delegate TResult Func<in T, out TResult>(T arg);

    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);

    // ...

    public delegate void Action();

    public delegate void Action<in T>(T obj);

    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);

    // ...
}

namespace System.Collections.Generic
{
    public interface IList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        T this[int index] { get; set; }
        // Indexer getter is compiled to get_Item. T is covariant.
        // Indexer setter is compiled to set_Item. T is contravariant.

        // Other members.
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerator<out T> : IDisposable, IEnumerator
    {
        T Current { get; } // Compiled to get_Current.T is covariant.
    }

    public interface IEnumerable<out T> : IEnumerable
    {
        IEnumerator<T> GetEnumerator();
    }
}

namespace System.Linq
{
    using System.Collections.Generic;

    public static class Enumerable
    {
        public static IEnumerable<TSource> Concat<TSource>(
            this IEnumerable<TSource> first, IEnumerable<TSource> second);

        public static IEnumerable<TResult> Select<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, TResult> selector);
    }
}

namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IQueryable : IEnumerable
    {
        Type ElementType { get; }

        Expression Expression { get; }

        IQueryProvider Provider { get; }
    }

    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable { }
}
#endif
