namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal static partial class Immutability
    {
        internal static void ConstantLocal()
        {
            const int ImmutableInt32 = 1;
            const int ImmutableInt32Sum = ImmutableInt32 + 2;
            // Constant expression ImmutableInt32 + 2 is compiled to: 3.
            const DayOfWeek ImmutableDayOfWeek = DayOfWeek.Saturday;
            const decimal ImmutableDecimal = (1M + 2M) * 3M;
            const string ImmutableString = "https://weblogs.asp.net/dixin";
            const string ImmutableStringConcat = "https://" + "flickr.com/dixin";
            const Uri ImmutableUri = null;
            // Reassignment to above constant locals cannot be compiled.

            int variableInt32 = Math.Max(ImmutableInt32, ImmutableInt32Sum);
            // Compiled to: Math.Max(1, 3).
            Trace.WriteLine(ImmutableString);
            // Compiled to: Trace.WriteLine("https://weblogs.asp.net/dixin").
        }

        internal enum Day
        {
            Sun, Mon, Tue, Wed, Thu, Fri, Sat
        }
        // Compiled to:
        internal enum CompiledDay : int
        {
            Sun = 0, Mon = 1, Tue = 2, Wed = 3, Thu = 4, Fri = 5, Sat = 6
        }

        internal static void Enumeration()
        {
            Trace.WriteLine((int)Day.Mon); // Compiled to: Trace.WriteLine(1).
            Trace.WriteLine(Day.Mon + 2); // Compiled to: Trace.WriteLine(Day.Wed).
            Trace.WriteLine((int)Day.Mon + Day.Thu); // Compiled to: Trace.WriteLine(Day.Fri).
            Trace.WriteLine(Day.Sat - Day.Mon); // Compiled to: Trace.WriteLine(5).
        }

        internal static void Using(Func<IDisposable> disposableFactory)
        {
            using (IDisposable immutableDisposable = disposableFactory())
            {
                // Reassignment to immutableDisposable cannot be compiled.
            }
        }

        internal static void ForEach<T>(IEnumerable<T> sequence)
        {
            foreach (T immutableValue in sequence)
            {
                // Reassignment to immutableValue cannot be compiled.
            }
        }
    }

    internal partial class Device
    {
        internal void InstanceMethod()
        {
            // Reassignment to this cannot be compiled.
        }
    }

    internal static partial class Immutability
    {
        internal static void InputAndOutput<T>(Span<T> span)
        {
            ref readonly T First(in Span<T> immutableInput)
            {
                // Reassignment to immutableInput cannot be compiled.
                return ref immutableInput[0];
            }

            ref readonly T immutableOutput = ref First(in span);
            // Reassignment to immutableOutput cannot be compiled.
        }

        internal static void ImmutableAlias()
        {
            int value = 1;
            int copyOfValue = value; // Copy.
            copyOfValue = 10; // After the assignment, value does not mutate.
            ref int aliasOfValue = ref value; // Mutable alias.
            aliasOfValue = 10; // After the reassignment, value mutates.
            ref readonly int immutableAliasOfValue = ref value; // Immutable alias.
            // Reassignment to immutableAliasOfValue cannot be compiled.

            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            Uri copyOfReference = reference; // Copy.
            copyOfReference = new Uri("https://flickr.com/dixin"); // After the assignment, reference does not mutate.
            ref Uri aliasOfReference = ref reference; // Mutable alias.
            aliasOfReference = new Uri("https://flickr.com/dixin"); // After the reassignment, reference mutates.
            ref readonly Uri immutableAliasOfReference = ref reference; // Immutable alias.
            // Reassignment to immutableAliasOfReference cannot be compiled.
        }

        internal static void QueryExpression(IEnumerable<int> source1, IEnumerable<int> source2)
        {
            IEnumerable<IGrouping<int, int>> query =
                from immutable1 in source1
                    // Reassignment to immutable1 cannot be compiled.
                join immutable2 in source2 on immutable1 equals immutable2 into immutable3
                // Reassignment to immutable2, immutable3 cannot be compiled.
                let immutable4 = immutable1
                // Reassignment to immutable4 cannot be compiled.
                group immutable4 by immutable4 into immutable5
                // Reassignment to immutable5 cannot be compiled.
                select immutable5 into immutable6
                // Reassignment to immutable6 cannot be compiled.
                select immutable6;
        }

        internal partial class ImmutableDevice
        {
            private readonly string name;

            private readonly decimal price;
        }

        internal partial class MutableDevice
        {
            internal string Name { get; set; }

            internal decimal Price { get; set; }
        }

        internal partial class ImmutableDevice
        {
            internal ImmutableDevice(string name, decimal price)
            {
                this.Name = name;
                this.Price = price;
            }

            internal string Name { get; }

            internal decimal Price { get; }
        }

        internal static void DevicePriceDrop()
        {
            MutableDevice mutableDevice = new MutableDevice() { Name = "Surface Laptop", Price = 799M };
            mutableDevice.Price -= 50M;

            ImmutableDevice immutableDevice = new ImmutableDevice(name: "Surface Book", price: 1199M);
            immutableDevice = new ImmutableDevice(name: immutableDevice.Name, price: immutableDevice.Price - 50M);
        }

        internal partial class MutableDevice
        {
            internal MutableDevice Discount()
            {
                this.Price = this.Price * 0.9M;
                return this;
            }
        }

        internal partial class ImmutableDevice
        {
            internal ImmutableDevice Discount() =>
                new ImmutableDevice(name: this.Name, price: this.Price * 0.9M);
        }

        internal partial struct Complex
        {
            internal Complex(double real, double imaginary)
            {
                this.Real = real;
                this.Imaginary = imaginary;
            }

            internal double Real { get; }

            internal double Imaginary { get; }
        }

        internal partial struct Complex
        {
            internal Complex(Complex value) => this = value; // Can reassign to this.

            internal Complex Value
            {
                get => this;
                set => this = value; // Can reassign to this.
            }

            internal Complex ReplaceBy(Complex value) => this = value; // Can reassign to this.

            internal Complex Mutate(double real, double imaginary) =>
                this = new Complex(real, imaginary); // Can reassign to this.
        }

        internal static void Structure()
        {
            Complex complex = new Complex(1, 1);
            complex.Real.WriteLine(); // 1
            complex.ReplaceBy(new Complex(2, 2));
            complex.Real.WriteLine(); // 2
            complex.Mutate(3, 3);
            complex.Real.WriteLine(); // 3
        }

        internal readonly partial struct ImmutableComplex
        {
            internal ImmutableComplex(double real, double imaginary)
            {
                this.Real = real;
                this.Imaginary = imaginary;
            }

            internal ImmutableComplex(in ImmutableComplex value) =>
                this = value; // Can reassign to this only in constructor.

            internal double Real { get; }

            internal double Imaginary { get; }

            internal void InstanceMethod(in ImmutableComplex value)
            {
                // Cannot reassign to this.
            }
        }

#if DEMO
        [IsReadOnly]
#endif
        internal struct CompiledImmutableComplex
        {
            // Members.
        }

        internal static void AnonymousType()
        {
            var immutableDevice = new { Name = "Surface Book", Price = 1199M };
        }

        [CompilerGenerated]
        [DebuggerDisplay(@"\{ Name = {Name}, Price = {Price} }", Type = "<Anonymous Type>")]
        internal sealed class AnonymousType0<TName, TPrice>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly TName name;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private readonly TPrice price;

            [DebuggerHidden]
            public AnonymousType0(TName name, TPrice price)
            {
                this.name = name;
                this.price = price;
            }

            public TName Name => this.name;

            public TPrice Price => this.price;

            [DebuggerHidden]
            public override bool Equals(object obj)
            {
                AnonymousType0<TName, TPrice> other = obj as AnonymousType0<TName, TPrice>;
                return other != null
                       && EqualityComparer<TName>.Default.Equals(this.name, other.name)
                       && EqualityComparer<TPrice>.Default.Equals(this.price, other.price);
            }

            // Other members.
        }

        internal static void CompiledAnonymousType()
        {
            AnonymousType0<string, decimal> immutableDevice = new AnonymousType0<string, decimal>(
                name: "Surface Book", price: 1199M);
        }

        internal static void ReuseAnonymousType()
        {
            var device1 = new { Name = "Surface Book", Price = 1199M };
            var device2 = new { Name = "Surface Pro", Price = 899M };
            var device3 = new { Name = "Xbox One", Price = 399 }; // Price is of type double.
            var device4 = new { Price = 174.99M, Name = "Surface Laptop" }; // Price is before Name.
            (device1.GetType() == device2.GetType()).WriteLine(); // True
            (device1.GetType() == device3.GetType()).WriteLine(); // False
            (device1.GetType() == device4.GetType()).WriteLine(); // False
        }

        internal static void PropertyInference(Uri uri, int value)
        {
            var anonymous1 = new { value, uri.Host };
            var anonymous2 = new { value = value, Host = uri.Host };
        }

        internal static void AnonymousTypeParameter()
        {
            var source = // Compiled to: AnonymousType0<string, decimal>[] source =.
                new[]
                {
                    new { Name = "Surface Book", Price = 1199M },
                    new { Name = "Surface Pro", Price = 899M }
                };
            var query = // Compiled to: <AnonymousType0<string, decimal>> query =.
                source.Where(device => device.Price > 0);
        }

        internal static void Let(IEnumerable<int> source)
        {
            IEnumerable<double> query =
                from immutable1 in source
                let immutable2 = Math.Sqrt(immutable1)
                select immutable1 + immutable2;
        }

        internal static void CompiledLet(IEnumerable<int> source)
        {
            IEnumerable<double> query = source
                .Select(immutable1 => new { immutable1, immutable2 = Math.Sqrt(immutable1) }) // let clause.
                .Select(context => context.immutable1 + context.immutable2); // select clause.
        }

        internal static void LocalVariable(IEnumerable<int> source, string path)
        {
            var a = default(int); // int.
            var b = 1M; // decimal.
            var c = typeof(void); // Type.
            var d = from int32 in source where int32 > 0 select Math.Sqrt(int32); // IEnumerable<double>.
            var e = File.ReadAllLines(path); // string[].
        }

        internal static void LocalVariableWithType()
        {
            var f = (Uri)null;
            var g = (Func<int, int>)(int32 => int32 + 1);
            var h = (Expression<Func<int, int>>)(int32 => int32 + 1);
        }

        internal static void TypesOfValues()
        {
            ValueTuple<string, decimal> tuple = new ValueTuple<string, decimal>("Surface Book", 1199M);
            string[] array = { "Surface Book", "1199M" };
            List<string> list = new List<string>() { "Surface Book", "1199M" };
        }

        internal static ValueTuple<string, decimal> Function(ValueTuple<string, decimal> values)
        {
            ValueTuple<string, decimal> variable1;
            ValueTuple<string, decimal> variable2 = default;
            IEnumerable<ValueTuple<string, decimal>> variable3;
            return values;
        }

#if DEMO
        internal static var Function(var values) // Cannot be compiled.
        {
            var variable1; // Cannot be compiled.
            var variable2 = default; // Cannot be compiled.
            IEnumerable<var> variable3; // Cannot be compiled.
            return values;
        }
#endif

        internal static void TupleLiteral()
        {
            (string, decimal) tuple1 = ("Surface Pro", 899M);
            // Compiled to: 
            // ValueTuple<string, decimal> tuple1 = new ValueTuple<string, decimal>("Surface Pro", 899M);

            (int, bool, (string, decimal)) tuple2 = (1, true, ("Surface Studio", 2999M));
            // ValueTuple<int, bool, ValueTuple<string, decimal>> tuple2 = 
            //    new ValueTuple<int, bool, ValueTuple<string, decimal>>(1, true, new ValueTuple<string, decimal>("Surface Studio", 2999M));
        }

        internal static (string, decimal) OutputMultipleValues()
        // Compiled to: internal static ValueTuple<string, decimal> OutputMultipleValues()
        {
            string value1 = default;
            int value2 = default;

            (string, decimal) Function() => (value1, value2);
            // Compiled to: ValueTuple<string, decimal> Function() => new ValueTuple<string, decimal>(value1, value2);

            Func<(string, decimal)> function = () => (value1, value2);
            // Compiled to: Func<ValueTuple<string, decimal>> function = () => new ValueTuple<string, decimal>(value1, value2);

            return (value1, value2);
            // Compiled to : new ValueTuple<string, decimal>(value1, value2);
        }

        internal static void ElementName()
        {
            (string Name, decimal Price) tuple1 = ("Surface Pro", 899M);
            tuple1.Name.WriteLine();
            tuple1.Price.WriteLine();
            // Compiled to: 
            // ValueTuple<string, decimal> tuple1 = new ValueTuple<string, decimal>("Surface Pro", 899M);
            // TraceExtensions.WriteLine(tuple1.Item1);
            // TraceExtensions.WriteLine(tuple1.Item2);

            (string Name, decimal Price) tuple2 = (ProductNanme: "Surface Book", ProductPrice: 1199M);
            tuple2.Name.WriteLine(); // Element names on the right side are ignored when there are element names on the left side.

            var tuple3 = (Name: "Surface Studio", Price: 2999M);
            tuple3.Name.WriteLine(); // Element names are available through var.

            ValueTuple<string, decimal> tuple4 = (Name: "Xbox One", Price: 179M);
            tuple4.Item1.WriteLine(); // Element names are not available on ValueTuple<T1, T2> type.
            tuple4.Item2.WriteLine();

            (string Name, decimal Price) Function((string Name, decimal Price) tuple)
            {
                tuple.Name.WriteLine(); // Input tuple’s element names are available in function.
                return (tuple.Name, tuple.Price - 10M);
            };
            var tuple5 = Function(("Xbox One", 299M));
            tuple5.Name.WriteLine(); // Output tuple’s element names are available through var.
            tuple5.Price.WriteLine();

            Func<(string Name, decimal Price), (string Name, decimal Price)> function = tuple =>
            {
                tuple.Name.WriteLine(); // Input tuple’s element names are available in function.
                return (tuple.Name, tuple.Price - 100M);
            };
            (string ProductName, decimal ProductPrice) tuple6 = function(("HoloLens", 3000M));
            tuple6.ProductName.WriteLine(); // Element names on the right side are ignored when there are element names on the left side.
            tuple6.ProductPrice.WriteLine();
        }

        internal static void ElementInference(Uri uri, int value)
        {
            var tuple1 = (value, uri.Host);
            var tuple2 = (value: value, Host: uri.Host);
        }

        internal static void DeconstructTuple()
        {
            (string, decimal) GetProduct() => ("HoloLens", 3000M);
            var (name, price) = GetProduct();
            name.WriteLine();
            price.WriteLine();
        }
    }

    internal partial class Device
    {
        internal void Deconstruct(out string name, out string description, out decimal price)
        {
            name = this.Name;
            description = this.Description;
            price = this.Price;
        }
    }

    internal static class DeviceExtensions
    {
        internal static void Deconstruct(this Device device, out string name, out string description, out decimal price)
        {
            name = device.Name;
            description = device.Description;
            price = device.Price;
        }
    }

    internal static partial class Immutability
    {
        internal static void DeconstructDevice()
        {
            Device GetDevice() => new Device() { Name = "Surface Studio", Description = "All-in-one PC.", Price = 2999M };
            var (name, description, price) = GetDevice();
            // Compiled to:
            // string name; string description; decimal price;
            // surfaceStudio.Deconstruct(out name, out description, out price);
            name.WriteLine();
            description.WriteLine();
            price.WriteLine();
        }

        internal static void Discard()
        {
            Device GetDevice() => new Device() { Name = "Surface Studio", Description = "All-in-one PC.", Price = 2999M };
            var (_, _, price1) = GetDevice();
            price1.WriteLine();
            (_, _, decimal price2) = GetDevice();
            price2.WriteLine();
        }

        internal static void TupleAssignment(int value1, int value2)
        {
            (value1, value2) = (1, 2);
            // Compiled to:
            // value1 = 1; value2 = 2;

            (value1, value2) = (value2, value1);
            // Compiled to:
            // int temp1 = value1; int temp2 = value2;
            // value1 = temp2; value2 = temp1;
        }

        internal static int Fibonacci(int n)
        {
            (int a, int b) = (0, 1);
            for (int i = 0; i < n; i++)
            {
                (a, b) = (b, a + b);
            }
            return a;
        }

#if DEMO
        internal class ImmutableDevice
        {
            internal ImmutableDevice(string name, decimal price) =>
                (this.Name, this.Price) = (name, price);

            internal string Name { get; }

            internal decimal Price { get; }
        }
#endif

        internal static void ImmutableCollection()
        {
            ImmutableList<int> immutableList1 = ImmutableList.Create(1, 2, 3);
            ImmutableList<int> immutableList2 = immutableList1.Add(4); // Create a new collection.
            object.ReferenceEquals(immutableList1, immutableList2).WriteLine(); // False
        }

        internal static void ReadOnlyCollection()
        {
            List<int> mutableList = new List<int>() { 1, 2, 3 };
            ImmutableList<int> immutableList = ImmutableList.CreateRange(mutableList);
            ReadOnlyCollection<int> readOnlyCollection = new ReadOnlyCollection<int>(mutableList);

            mutableList.Add(4);
            immutableList.Count.WriteLine(); // 3
            readOnlyCollection.Count.WriteLine(); // 4
        }

        internal class Bundle
        {
            internal Bundle(MutableDevice device1, MutableDevice device2) =>
                (this.Device1, this.Device2) = (device1, device2);

            internal MutableDevice Device1 { get; }

            internal MutableDevice Device2 { get; }
        }

        internal static void ShallowImmutability()
        {
            MutableDevice device1 = new MutableDevice() { Name = "Surface Book", Price = 1199M };
            MutableDevice device2 = new MutableDevice() { Name = "HoloLens", Price = 3000M };
            Bundle bundle = new Bundle(device1, device2);
            // Reassignment to bundle.Device1, bundle.Device2 cannot be compiled.

            bundle.Device1.Name = "Surface Studio";
            bundle.Device1.Price = 2999M;
            bundle.Device2.Price -= 50M;
        }

#if DEMO
        internal class Bundle
        {
            internal Bundle(ImmutableDevice device1, ImmutableDevice device2) =>
                (this.Device1, this.Device2) = (device1, device2);

            internal ImmutableDevice Device1 { get; }

            internal ImmutableDevice Device2 { get; }
        }
#endif
    }
}

#if DEMO
namespace System
{
    using System.Runtime.Serialization;

    public struct DateTime : IComparable, IComparable<DateTime>, IConvertible, IEquatable<DateTime>, IFormattable, ISerializable
    {
        private const int DaysPerYear = 365;
        // Compiled to:
        // .field private static literal int32 DaysPerYear = 365

        private const int DaysPer4Years = DaysPerYear * 4 + 1;
        // Compiled to:
        // .field private static literal int32 DaysPer4Years = 1461

        // Other members.
    }
}

namespace System
{
    using System.Collections;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class Tuple<T1, T2> : IStructuralEquatable, IStructuralComparable, IComparable, ITupleInternal, ITuple
    {
        private readonly T1 m_Item1;

        private readonly T2 m_Item2;

        public Tuple(T1 item1, T2 item2)
        {
            this.m_Item1 = item1;
            this.m_Item2 = item2;
        }

        public T1 Item1 => this.m_Item1;

        public T2 Item2 => this.m_Item2;

        // Other members.
    }
}

namespace System
{
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Auto)]
    public struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>, IStructuralEquatable, IStructuralComparable, IComparable, IComparable<ValueTuple<T1, T2>>, IValueTupleInternal, ITuple
    {
        public T1 Item1;

        public T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        public override string ToString() => 
            "(" + this.Item1?.ToString() + ", " + this.Item2?.ToString() + ")";

        // Other members.
    }
}
#endif
