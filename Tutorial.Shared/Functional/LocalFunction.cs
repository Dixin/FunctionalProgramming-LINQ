namespace Tutorial.Functional
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static partial class LocalFunctions
    {
        internal static void MethodWithLocalFunction()
        {
            void LocalFunction() // Define local function.
            {
                MethodBase.GetCurrentMethod().Name.WriteLine();
            }
            LocalFunction(); // Call local function.
        }

        internal static int PropertyWithLocalFunction
        {
            get
            {
                LocalFunction(); // Call local function.
                void LocalFunction() // Define local function.
                {
                    MethodBase.GetCurrentMethod().Name.WriteLine();
                }
                LocalFunction(); // Call local function.
                return 0;
            }
        }

        [CompilerGenerated]
        internal static void CompiledLocalFunction()
        {
            MethodBase.GetCurrentMethod().Name.WriteLine();
        }

        internal static void CompiledMethodWithLocalFunction()
        {
            CompiledLocalFunction();
        }

#if DEMO
        internal static void LocalFunctionOverload()
        {
            void LocalFunction() { }
            void LocalFunction(int int32) { } // Cannot be compiled.
        }
#endif

        internal static int BinarySearch<T>(this IList<T> source, T value, IComparer<T> comparer = null)
        {
            return BinarySearch(source, value, comparer ?? Comparer<T>.Default, 0, source.Count - 1);
        }

        private static int BinarySearch<T>(IList<T> source, T value, IComparer<T> comparer, int startIndex, int endIndex)
        {
            if (startIndex > endIndex) { return -1; }
            int middleIndex = startIndex + (endIndex - startIndex) / 2;
            int compare = comparer.Compare(source[middleIndex], value);
            if (compare == 0) { return middleIndex; }
            return compare > 0
                ? BinarySearch(source, value, comparer, startIndex, middleIndex - 1)
                : BinarySearch(source, value, comparer, middleIndex + 1, endIndex);
        }

        internal static int BinarySearchWithLocalFunction<T>(this IList<T> source, T value, IComparer<T> comparer = null)
        {
            int BinarySearch(
                IList<T> localSource, T localValue, IComparer<T> localComparer, int startIndex, int endIndex)
            {
                if (startIndex > endIndex) { return -1; }
                int middleIndex = startIndex + (endIndex - startIndex) / 2;
                int compare = localComparer.Compare(localSource[middleIndex], localValue);
                if (compare == 0) { return middleIndex; }
                return compare > 0
                    ? BinarySearch(localSource, localValue, localComparer, startIndex, middleIndex - 1)
                    : BinarySearch(localSource, localValue, localComparer, middleIndex + 1, endIndex);
            }

            return BinarySearch(source, value, comparer ?? Comparer<T>.Default, 0, source.Count - 1);
        }

        internal static int BinarySearchWithClosure<T>(this IList<T> source, T value, IComparer<T> comparer = null)
        {
            int BinarySearch(int startIndex, int endIndex)
            {
                if (startIndex > endIndex) { return -1; }
                int middleIndex = startIndex + (endIndex - startIndex) / 2;
                int compare = comparer.Compare(source[middleIndex], value);
                if (compare == 0) { return middleIndex; }
                return compare > 0
                    ? BinarySearch(startIndex, middleIndex - 1)
                    : BinarySearch(middleIndex + 1, endIndex);
            }

            comparer = comparer ?? Comparer<T>.Default;
            return BinarySearch(0, source.Count - 1);
        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        private struct Closure2<T>
        {
            public IComparer<T> Comparer;

            public IList<T> Source;

            public T Value;
        }

        [CompilerGenerated]
        private static int CompiledLocalBinarySearch<T>(int startIndex, int endIndex, ref Closure2<T> closure)
        {
            if (startIndex > endIndex) { return -1; }
            int middleIndex = startIndex + (endIndex - startIndex) / 2;
            int compare = closure.Comparer.Compare(closure.Source[middleIndex], closure.Value);
            if (compare == 0) { return middleIndex; }
            return compare <= 0
                ? CompiledLocalBinarySearch(middleIndex + 1, endIndex, ref closure)
                : CompiledLocalBinarySearch(startIndex, middleIndex - 1, ref closure);
        }

        internal static int CompiledBinarySearchWithClosure<T>(IList<T> source, T value, IComparer<T> comparer = null)
        {
            Closure2<T> closure = new Closure2<T>()
            {
                Source = source,
                Value = value,
                Comparer = comparer
            };
            return CompiledLocalBinarySearch(0, source.Count - 1, ref closure);
        }

        internal static void LocalFunctionWithLocalFunction()
        {
            void LocalFunction()
            {
                void NestedLocalFunction() { }
                NestedLocalFunction();
            }
            LocalFunction();
        }

        internal static Action AnonymousFunctionWithLocalFunction()
        {
            return () => // Return an anonymous function of type Action.
            {
                void LocalFunction() { }
                LocalFunction();
            };
        }

        internal class Closure
        {
#pragma warning disable SA1400 // Access modifier must be declared
            int field = 1; // Outside function Add.
#pragma warning restore SA1400 // Access modifier must be declared

            internal void Add()
            {
                int local = 2; // Inside function Add.
#pragma warning disable SA1101 // Prefix local calls with this
                (local + field).WriteLine(); // local + this.field.
#pragma warning restore SA1101 // Prefix local calls with this
            }
        }

        internal static void LocalFunctionWithClosure()
        {
            int free = 1; // Outside local function Add.
            void Add()
            {
                int local = 2; // Inside local function Add.
                (local + free).WriteLine();
            }
            Add();
        }

        [CompilerGenerated]
        [StructLayout(LayoutKind.Auto)]
        private struct Closure1
        {
            public int Free;
        }

        [CompilerGenerated]
        private static void CompiledAdd(ref Closure1 closure)
        {
            int local = 2;
            (local + closure.Free).WriteLine();
        }

        internal static void CompiledLocalFunctionWithClosure()
        {
            int free = 1;
            Closure1 closure = new Closure1() { Free = free };
            CompiledAdd(ref closure);
        }

        internal static void FreeVariableMutation()
        {
            int free = 1;

            void Add()
            {
                int local = 2;
                (local + free).WriteLine();
            }

            Add(); // 3
            free = 3; // Free variable mutates.
            Add(); // 5
        }

        internal static void CompiledFreeVariableMutation()
        {
            int free = 1;
            Closure1 closure = new Closure1 { Free = free };
            CompiledAdd(ref closure);
            closure.Free = free = 3;
            CompiledAdd(ref closure);
        }

        internal static void FreeVariableReference()
        {
            List<Action> localFunctions = new List<Action>();
            for (int free = 0; free < 3; free++) // free is 0, 1, 2.
            {
                void LocalFunction() { free.WriteLine(); }
                localFunctions.Add(LocalFunction);
            } // free is 3.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 3 3 3 (instead of 0 1 2)
            }
        }

        [CompilerGenerated]
        private struct Closure3
        {
            public int Free;

            internal void LocalFunction() { this.Free.WriteLine(); }
        }

        internal static void CompiledFreeVariableReference()
        {
            List<Action> localFunctions = new List<Action>();
            Closure3 closure = new Closure3();
            for (closure.Free = 0; closure.Free < 3; closure.Free++) // free is 0, 1, 2.
            {
                localFunctions.Add(closure.LocalFunction);
            } // closure.Free is 3.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 3 3 3 (instead of 0 1 2)
            }
        }

        internal static void CopyFreeVariableReference()
        {
            List<Action> localFunctions = new List<Action>();
            for (int free = 0; free < 3; free++) // free is 0, 1, 2.
            {
                int copyOfFree = free;
                // When free mutates, copyOfFree does not mutate.
                void LocalFunction() { copyOfFree.WriteLine(); }
                localFunctions.Add(LocalFunction);
            } // free is 3. copyOfFree is 0, 1, 2.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 0 1 2
            }
        }

        [CompilerGenerated]
        private sealed class Closure4
        {
            public int CopyOfFree;

            internal void LocalFunction() { this.CopyOfFree.WriteLine(); }
        }

        internal static void CompiledCopyFreeVariableReference()
        {
            List<Action> localFunctions = new List<Action>();
            for (int free = 0; free < 3; free++)
            {
                Closure4 closure = new Closure4() { CopyOfFree = free }; // free is 0, 1, 2.
                // When free changes, closure.CopyOfFree does not change.
                localFunctions.Add(closure.LocalFunction);
            } // free is 3. closure.CopyOfFree is 0, 1, 2.
            foreach (Action localFunction in localFunctions)
            {
                localFunction(); // 0 1 2
            }
        }

        private static Action persisted;

        internal static void FreeVariableLifetime()
        {
            // https://msdn.microsoft.com/en-us/library/System.Array.aspx
            byte[] tempLargeInstance = new byte[0x_7FFF_FFC7]; // Local variable of large instance, Array.MaxByteArrayLength is 0x_7FFF_FFC7.
            // ...
            void LocalFunction()
            {
                // ...
                int length = tempLargeInstance.Length; // Reference to free variable.
                // ...
                length.WriteLine();
                // ...
            }
            // ...
            LocalFunction();
            // ...
            persisted = LocalFunction; // Reference to local function.
        }

        [CompilerGenerated]
        private sealed class Closure5
        {
            public byte[] TempLargeInstance;

            internal void LocalFunction()
            {
                int length = this.TempLargeInstance.Length;
                length.WriteLine();
            }
        }

        internal static void CompiledFreeVariableLifetime()
        {
            byte[] tempLargeInstance = new byte[0X7FFFFFC7];
            Closure5 closure = new Closure5() { TempLargeInstance = tempLargeInstance };
            closure.LocalFunction();
            persisted = closure.LocalFunction;
            // closure's lifetime is bound to persisted, so is closure.TempLargeInstance.
        }

        internal static void FreeVariableLifetimeOptimized()
        {
            byte[] tempLargeInstance = new byte[0x_7FFF_FFC7];
            int length = tempLargeInstance.Length;
            void LocalFunction() { length.WriteLine(); }
            LocalFunction();
            persisted = LocalFunction; // Reference to local function.
        }

        internal static Action SharedClosure()
        {
            byte[] tempLargeInstance = new byte[0x_7FFF_FFC7];
            void LocalFunction1() { int length = tempLargeInstance.Length; }
            LocalFunction1();

            bool tempSmallInstance = false;
            void LocalFunction2() { tempSmallInstance = true; }
            LocalFunction2();

            return LocalFunction2; // Return a function of Action type.
        }

        internal static void CallSharedClosure()
        {
            persisted = SharedClosure(); // Returned LocalFunction2 is persisted.
        }

        [CompilerGenerated]
        private struct Closure6
        {
            public byte[] TempLargeInstance;

            internal void LocalFunction1() { int length = this.TempLargeInstance.Length; }

            public bool TempSmallInstance;

            internal void LocalFunction2() { this.TempSmallInstance = true; }
        }

        internal static Action CompiledSharedClosure()
        {
            Closure6 closure = new Closure6();
            closure.TempLargeInstance = new byte[0x_7FFF_FFC7];
            closure.LocalFunction1();

            closure.TempSmallInstance = false;
            closure.LocalFunction2();

            return closure.LocalFunction2; // Return a function of Action type.
        }

        internal static Action SeparatedClosures()
        {
            { // Lexical scope has its own closure.
                byte[] tempLargeInstance = new byte[0x_7FFF_FFC7];
                void LocalFunction1() { int length = tempLargeInstance.Length; }
                LocalFunction1();
            }

            bool tempSmallInstance = false;
            void LocalFunction2() { tempSmallInstance = true; }
            LocalFunction2();

            return LocalFunction2; // Return a function of Action type.
        }
    }
}
