namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;

    using Mono.Cecil;

    internal static partial class Purity
    {
        [Pure]
        internal static bool IsPositive(int int32) => int32 > 0;

        // Impure.
        internal static bool IsNegative(int int32)
        {
            Console.WriteLine(int32); // Side effect: console I/O.
            return int32 < 0;
        }

        [Pure]
        internal static class PureFunctions
        {
            // Pure.
            internal static int Increase(int int32) => int32 + 1;

            // Pure.
            internal static int Decrease(int int32) => int32 - 1;
        }

        internal static int DoubleWithPureContracts(int int32)
        {
            Contract.Requires<ArgumentOutOfRangeException>(IsPositive(int32)); // Function precondition.
            Contract.Ensures(IsPositive(Contract.Result<int>())); // Function post condition.

            return int32 + int32; // Function body.
        }

        internal static int DoubleWithImpureContracts(int int32)
        {
            Contract.Requires<ArgumentOutOfRangeException>(IsNegative(int32)); // Function precondition.
            Contract.Ensures(IsNegative(Contract.Result<int>())); // Function post condition.

            return int32 + int32; // Function body.
        }

        internal static void FunctionCount(
            string contractsAssemblyDirectory, string assemblyDirectory)
        {
            bool HasPureAttribute(ICustomAttributeProvider member) =>
                member.CustomAttributes.Any(attribute =>
                    attribute.AttributeType.FullName.Equals(typeof(PureAttribute).FullName, StringComparison.Ordinal));

            string[] contractsAssemblyPaths = Directory
                .EnumerateFiles(contractsAssemblyDirectory, "*.Contracts.dll")
                .ToArray();
            // Query the count of pure functions in all contracts assemblies, including all public functions in public type with [Pure], and all public function members with [Pure] in public types.
            int pureFunctionCount = contractsAssemblyPaths
                .Select(AssemblyDefinition.ReadAssembly)
                .SelectMany(contractsAssembly => contractsAssembly.Modules)
                .SelectMany(contractsModule => contractsModule.GetTypes())
                .Where(contractsType => contractsType.IsPublic)
                .SelectMany(contractsType => HasPureAttribute(contractsType)
                    ? contractsType.Methods.Where(contractsFunction => contractsFunction.IsPublic)
                    : contractsType.Methods.Where(contractsFunction =>
                        contractsFunction.IsPublic && HasPureAttribute(contractsFunction)))
                .Count();
            pureFunctionCount.WriteLine(); // 2223

            // Query the count of all public functions in public types in all FCL assemblies.
            int functionCount = contractsAssemblyPaths
                .Select(contractsAssemblyPath => Path.Combine(
                    assemblyDirectory,
                    Path.ChangeExtension(Path.GetFileNameWithoutExtension(contractsAssemblyPath), "dll")))
                .Select(AssemblyDefinition.ReadAssembly)
                .SelectMany(assembly => assembly.Modules)
                .SelectMany(module => module.GetTypes())
                .Where(type => type.IsPublic)
                .SelectMany(type => type.Methods)
                .Count(function => function.IsPublic);
            functionCount.WriteLine(); // 82566
        }

        [Pure] // Incorrect. No error or warning.
        internal static ProcessStartInfo Initialize(ProcessStartInfo processStart)
        {
            processStart.RedirectStandardInput = false;
            processStart.RedirectStandardOutput = false;
            processStart.RedirectStandardError = false;
            Console.WriteLine($"Initialized.");
            return processStart;
        }
    }
}

#if DEMO
namespace System
{
    public static class Math
    {
        public static int Max(int val1, int val2) => (val1 >= val2) ? val1 : val2;

        public static int Min(int val1, int val2) => (val1 <= val2) ? val1 : val2;
    }
}

namespace System
{
    using System.Diagnostics.Contracts;

    public static class Math
    {
        [Pure]
        public static int Abs(int value)
        {
            Contract.Requires(value != int.MinValue);
            Contract.Ensures(Contract.Result<int>() >= 0);
            Contract.Ensures((value - Contract.Result<int>()) <= 0);

            return default;
        }
    }
}
#endif
