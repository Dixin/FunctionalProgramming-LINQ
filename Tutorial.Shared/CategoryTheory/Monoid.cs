namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using Microsoft.FSharp.Core;

    public interface IMonoid<T>
    {
        static abstract T Multiply(T value1, T value2);

        static abstract T Unit { get; }
    }

    public class Int32SumMonoid : IMonoid<int>
    {
        public static int Multiply(int value1, int value2) => value1 + value2;

        public static int Unit => GenericIndirection.AdditiveUnit<int>(); // 0.
    }

    public class Int32ProductMonoid : IMonoid<int>
    {
        public static int Multiply(int value1, int value2) => value1 * value2;

        public static int Unit => GenericIndirection.MultiplicativeUnit<int>(); // 1.
    }

    public static class GenericIndirection
    {
        public static T AdditiveUnit<T>() where T : IAdditiveIdentity<T, T> => T.AdditiveIdentity;

        public static T MultiplicativeUnit<T>() where T : IMultiplicativeIdentity<T, T> => T.MultiplicativeIdentity;
    }

    public class ClockMonoid : IMonoid<uint>
    {
        public static uint Multiply(uint value1, uint value2)
        {
            uint result = (value1 + value2) % Unit;
            return result != 0 ? result : Unit;
        }

        public static uint Unit => 12U;
    }

    public class StringConcatMonoid : IMonoid<string>
    {
        public static string Multiply(string value1, string value2) => string.Concat(value1, value2);

        public static string Unit => string.Empty;
    }

    public class EnumerableConcatMonoid<T> : IMonoid<IEnumerable<T>>
    {
        public static IEnumerable<T> Multiply(IEnumerable<T> value1, IEnumerable<T> value2) => value1.Concat(value2);

        public static IEnumerable<T> Unit => Enumerable.Empty<T>();
    }

    public class BooleanAndMonoid : IMonoid<bool>
    {
        public static bool Multiply(bool value1, bool value2) => value1 && value2;

        public static bool Unit => true;
    }

    public class BooleanOrMonoid : IMonoid<bool>
    {
        public static bool Multiply(bool value1, bool value2) => value1 || value2;

        public static bool Unit => false;
    }

#if DEMO
    public class VoidMonoid : IMonoid<void>
    {
        public static void Multiply(void value1, void value2) => default;

        public static void Unit() => default;
    }
#endif

    public class UnitMonoid : IMonoid<Unit>
    {
        public static Unit Multiply(Unit value1, Unit value2) => null;

        public static Unit Unit => null;
    }

    public class MonoidCategory<T, TMonoid> : ICategory<Type, T> where TMonoid : IMonoid<T>
    {
        public static IEnumerable<Type> Objects { get { yield return typeof(TMonoid); } }

        public static T Compose(T morphism2, T morphism1) => TMonoid.Multiply(morphism1, morphism2);

        public static T Id(Type @object) => TMonoid.Unit;
    }
}

#if DEMO
namespace System
{
    using System.Runtime.InteropServices;

	[ComVisible(true)]
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Void
	{
	}
}

namespace Microsoft.FSharp.Core
{
    using System;

    [CompilationMapping(SourceConstructFlags.ObjectType)]
    [Serializable]
    public sealed class Unit : IComparable
    {
        internal Unit() { }

        public override int GetHashCode() => 0;

        public override bool Equals(object obj) => 
            obj == null || LanguagePrimitives.IntrinsicFunctions.TypeTestGeneric<Unit>(obj);

        int IComparable.CompareTo(object obj) => 0;
    }
}
#endif
