namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static partial class AnonymousFunctions
    {
        internal static bool IsPositive(int int32) { return int32 > 0; }

        internal static void NamedFunction()
        {
            Func<int, bool> isPositive = IsPositive;
            bool result = isPositive(0);
        }

        internal static void AnonymousMethod()
        {
            Func<int, bool> isPositive = delegate (int int32) { return int32 > 0; };
            bool result = isPositive(0);
        }

        internal static void AnonymousFunction()
        {
            Func<int, bool> isPositive = (int32) => { return int32 > 0; };
            // Equivalent to: Func<int, bool> isPositive = int32 => int32 > 0;
            bool result = isPositive(0);
        }

        [CompilerGenerated]
        private static Func<int, bool> cachedIsPositive;

        [CompilerGenerated]
        private static bool CompiledIsPositive(int int32) { return int32 > 0; }

        internal static void CompiledAnonymousFunction()
        {
            Func<int, bool> isPositive;
            if (cachedIsPositive == null)
            {
                cachedIsPositive = new Func<int, bool>(CompiledIsPositive);
            }
            isPositive = cachedIsPositive;
            bool result = isPositive.Invoke(0);
        }

        internal static void ExpressionLambda()
        {
            Func<int, int, int> add = (int32A, int32B) => int32A + int32B;
            Func<int, bool> isPositive = int32 => int32 > 0;
            Action<int> traceLine = int32 => int32.WriteLine();
        }

        internal static void StatementLambda()
        {
            Func<int, int, int> add = (int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            };
            Func<int, bool> isPositive = int32 =>
            {
                int32.WriteLine();
                return int32 > 0;
            };
            Action<int> traceLine = int32 =>
            {
                int32.WriteLine();
                Trace.Flush();
            };
        }

        internal static void Constructor()
        {
            Func<int, bool> isPositive = new Func<int, bool>(int32 => int32 > 0);
            bool result = isPositive(0);
        }

        internal static void Conversion()
        {
            Func<int, bool> isPositive = (Func<int, bool>)(int32 => int32 > 0);
            bool result = isPositive(0);
        }

        internal static void Simplified()
        {
            Func<int, bool> isPositive = int32 => int32 > 0;
            bool result = isPositive(0);
        }

#if DEMO
        internal static void CallAnonymousFunction(int arg)
        {
            (int32 => int32 > 0)(arg); // Cannot be compiled.
        }
#endif

        internal static void CallAnonymousFunctionWithConstructor(int arg)
        {
            bool result = new Func<int, bool>(int32 => int32 > 0)(arg);
        }

        internal static void CallAnonymousFunctionWithConversion(int arg)
        {
            bool result = ((Func<int, bool>)(int32 => int32 > 0))(arg);
        }

        [CompilerGenerated]
        [Serializable]
        private sealed class Functions
        {
            public static readonly Functions Singleton = new Functions();

            public static Func<int, bool> cachedIsPositive;

            internal bool IsPositive(int int32) { return int32 > 0; }
        }

        internal static void CompiledCallLambdaExpressionWithConstructor()
        {
            Func<int, bool> isPositive;
            if (Functions.cachedIsPositive == null)
            {
                Functions.cachedIsPositive = new Func<int, bool>(Functions.Singleton.IsPositive);
            }
            isPositive = Functions.cachedIsPositive;
            bool result = isPositive.Invoke(1);
        }

        internal static void ImmediatelyInvokedFunctionExpression()
        {
            new Func<int, int, int>((int32A, int32B) => int32A + int32B)(1, 2);
            new Action<int>(int32 => int32.WriteLine())(1);

            new Func<int, int, int>((int32A, int32B) =>
            {
                int sum = int32A + int32B;
                return sum;
            })(1, 2);
            new Func<int, bool>(int32 =>
            {
                int32.WriteLine();
                return int32 > 0;
            })(1);
            new Action<int>(int32 =>
            {
                int32.WriteLine();
                Trace.Flush();
            })(1);
        }

        internal static void AnonymousFunctionWithClosure()
        {
            int free = 1; // Outside the scope of function add.
            new Action(() =>
            {
                int local = 2; // Inside the scope of function add.
                (local + free).WriteLine();
            })(); // 3
        }

        [CompilerGenerated]
        private sealed class Closure1
        {
            public int Free;

            internal void Add()
            {
                int local = 2;
                (local + this.Free).WriteLine();
            }
        }

        internal static void CompiledAnonymousFunctionWithClosure()
        {
            int free = 1;
            Closure1 closure = new Closure1() { Free = free };
            closure.Add(); // 3
        }

        internal partial class Data
        {
            private int value;

            static Data() => MethodBase.GetCurrentMethod().Name.WriteLine(); // Static constructor.

            internal Data(int value) => this.value = value; // Constructor.

            ~Data() => MethodBase.GetCurrentMethod().Name.WriteLine(); // Finalizer.

            internal bool InstanceEquals(Data other) => this.value == other?.value; // Instance method.

            internal static bool StaticEquals(Data @this, Data other) => @this?.value == other?.value; // Static method.

            public static Data operator +(Data data1, Data data2) => new Data(data1.value + data2.value); // Operator overload.

            public static explicit operator int(Data value) => value.value; // Explicit conversion operator.

            public static implicit operator Data(int value) => new Data(value); // Implicit conversion operator.

            internal int ReadOnlyValue => this.value; // Getter only property.

            internal int ReadWriteValue
            {
                get => this.value; // Property getter.
                set => this.value = value; // Property setter.
            }

            internal int this[long index] => throw new NotImplementedException(); // Getter only indexer.

            internal int this[int index]
            {
                get => throw new NotImplementedException(); // Indexer getter.
                set => throw new NotImplementedException(); // Indexer setter.
            }

            internal event EventHandler Created
            {
                add => MethodBase.GetCurrentMethod().Name.WriteLine(); // Event accessor.
                remove => MethodBase.GetCurrentMethod().Name.WriteLine(); // Event accessor.
            }

            internal int GetValue()
            {
                int LocalFunction() => this.value; // Local function.
                return LocalFunction();
            }
        }

        internal static partial class DataExtensions
        {
            internal static bool ExtensionEquals(Data @this, Data other) => @this?.ReadOnlyValue == other?.ReadOnlyValue; // Extension method.
        }

        internal partial class Data : IEquatable<Data>
        {
            bool IEquatable<Data>.Equals(Data other) => this.value == other?.value; // Explicit interface implementation.
        }
    }
}
