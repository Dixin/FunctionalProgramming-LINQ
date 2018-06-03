namespace Tutorial.Functional
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static partial class InputOutput
    {
        internal static void InputByCopy(Uri reference, int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
        }

        internal static void CallInputByCopy()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            InputByCopy(reference, value); // Copied.
            reference.WriteLine(); // https://weblogs.asp.net/dixin
            value.WriteLine(); // 1
        }

        internal static void InputByAlias(ref Uri reference, ref int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
        }

        internal static void CallInputByAlias()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            InputByAlias(ref reference, ref value); // Not copied.
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void InputByImmutableAlias(in Uri reference, in int value)
        {
        }

        internal static void CallInputByImmutableAlias()
        {
            Uri reference = new Uri("https://weblogs.asp.net/dixin");
            int value = 1;
            InputByImmutableAlias(in reference, in value); // Not copied.
            InputByImmutableAlias(reference, value); // Not copied.
            reference.WriteLine(); // https://weblogs.asp.net/dixin
            value.WriteLine(); // 1
        }

#if DEMO
        internal static void InputByImmutableAlias(in Uri reference, in int value)
        {
            reference = new Uri("https://flickr.com/dixin"); // Cannot be compiled.
            value = 10; // Cannot be compiled.
        }
#endif

        internal static bool OutputParameter(out Uri reference, out int value)
        {
            reference = new Uri("https://flickr.com/dixin");
            value = 10;
            return false;
        }

        internal static void CallOutputParameter()
        {
            Uri reference;
            int value;
            OutputParameter(out reference, out value); // Not copied.
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void OutVariable()
        {
            OutputParameter(out Uri reference, out int value);
            reference.WriteLine(); // https://flickr.com/dixin
            value.WriteLine(); // 10
        }

        internal static void Discard()
        {
            bool result = OutputParameter(out _, out _);
            OutputParameter(out _, out _);
            _ = OutputParameter(out _, out _);
        }

        internal static int Sum(params int[] values)
        {
            int sum = 0;
            foreach (int value in values)
            {
                sum += value;
            }

            return sum;
        }

#if DEMO
        internal static int CompiledSum([ParamArray] int[] values)
        {
            int sum = 0;
            foreach (int value in values)
            {
                sum += value;
            }
            return sum;
        }
#endif

        internal static void CallSum(int[] array)
        {
            int sum1 = Sum();
            int sum2 = Sum(0);
            int sum3 = Sum(0, 1, 2, 3, 4);
            int sum4 = Sum(new[] { 0, 1, 2, 3, 4 });
        }

        internal static void CompiledCallSum(int[] array)
        {
            int sum1 = Sum(Array.Empty<int>());
            int sum2 = Sum(new int[] { 0 });
            int sum3 = Sum(new int[] { 0, 1, 2, 3, 4 });
            int sum4 = Sum(new int[] { 0, 1, 2, 3, 4 });
        }

        internal static void MultipleParameters(bool required1, int required2, params string[] optional)
        {
        }

        internal static void PositionalAndNamed()
        {
            InputByCopy(null, 0); // Positional arguments.
            InputByCopy(reference: null, value: 0); // Named arguments.
            InputByCopy(value: 0, reference: null); // Named arguments.
            InputByCopy(null, value: 0); // Positional argument followed by named argument.
            InputByCopy(reference: null, 0); // Named argument followed by positional argument.
        }

        internal static void CompiledPositionalAndNamed()
        {
            InputByCopy(null, 0);
            InputByCopy(null, 0);
            InputByCopy(null, 0);
            InputByCopy(null, 0);
            InputByCopy(null, 0);
        }

        internal static void NamedArgumentEvaluation()
        {
            InputByCopy(reference: GetUri(), value: GetInt32()); // Call GetUri then GetInt32.
            InputByCopy(value: GetInt32(), reference: GetUri()); // Call GetInt32 then GetUri.
        }

        internal static Uri GetUri() { return default; }

        internal static int GetInt32() { return default; }

        internal static void CompiledNamedArgumentEvaluation()
        {
            InputByCopy(GetUri(), GetInt32()); // Call GetUri then GetInt32.
            int value = GetInt32(); // Call GetInt32 then GetUri.
            InputByCopy(GetUri(), value);
        }

        internal static void Named()
        {
            UnicodeEncoding unicodeEncoding1 = new UnicodeEncoding(true, true, true);
            UnicodeEncoding unicodeEncoding2 = new UnicodeEncoding(
                bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true);
        }

        internal static void Optional(
            bool required1, char required2,
            int optional1 = int.MaxValue, string optional2 = "Default value.",
            Uri optional3 = null, Guid optional4 = new Guid(),
            Uri optional5 = default, Guid optional6 = default)
        {
        }

        internal static void CallOptional()
        {
            Optional(true, '@');
            Optional(true, '@', 1);
            Optional(true, '@', 1, string.Empty);
            Optional(true, '@', optional2: string.Empty);
            Optional(
                optional6: Guid.NewGuid(), optional3: GetUri(), required1: false, optional1: GetInt32(),
                required2: Convert.ToChar(64)); // Call Guid.NewGuid, then GetUri, then GetInt32, then Convert.ToChar.
        }

        internal static void CompiledCallOptional()
        {
            Optional(true, '@', 1, "Default value.", null, new Guid(), null, new Guid());
            Optional(true, '@', 1, "Default value.", null, new Guid(), null, new Guid());
            Optional(true, '@', 1, string.Empty, null, new Guid(), null, new Guid());
            Optional(true, '@', 1, string.Empty, null, new Guid(), null, new Guid());
            Guid optional6 = Guid.NewGuid(); // Call Guid.NewGuid, then GetUri, then GetInt32, then Convert.ToChar.
            Uri optional3 = GetUri();
            int optional1 = GetInt32();
            Optional(false, Convert.ToChar(64), optional1, "Default value.", optional3);
        }

        internal static void TraceWithCaller(
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Trace.WriteLine($"[{callerMemberName}, {callerFilePath}, {callerLineNumber}]: {message}");
        }

        internal static void CallTraceWithCaller()
        {
            TraceWithCaller("Message.");
            // Compiled to:
            // TraceWithCaller("Message.", "CompiledCallTraceWithCaller", @"D:\Data\GitHub\Tutorial\Tutorial.Shared\Functional\InputOutput.cs,", 216);
        }
    }

    internal static partial class InputOutput
    {
        internal static int FirstValueByCopy(int[] values)
        {
            return values[0];
        }

        internal static Uri FirstReferenceByCopy(Uri[] references)
        {
            return references[0];
        }

        internal static void OutputByCopy()
        {
            int[] values = new int[] { 0, 1, 2, 3, 4 };
            int firstValue = FirstValueByCopy(values); // Copy of values[0].
            firstValue = 10;
            values[0].WriteLine(); // 0

            Uri[] references = new Uri[] { new Uri("https://weblogs.asp.net/dixin") };
            Uri firstReference = FirstReferenceByCopy(references); // Copy of references[0].
            firstReference = new Uri("https://flickr.com/dixin");
            references[0].WriteLine(); // https://weblogs.asp.net/dixin
        }

        internal static ref int FirstValueByAlias(int[] values)
        {
            return ref values[0];
        }

        internal static ref Uri FirstReferenceByAlias(Uri[] references)
        {
            return ref references[0];
        }

        internal static void OutputByAlias()
        {
            int[] values = new int[] { 0, 1, 2, 3, 4 };
            ref int firstValue = ref FirstValueByAlias(values); // Alias of values[0].
            firstValue = 10;
            values[0].WriteLine(); // 10

            Uri[] references = new Uri[] { new Uri("https://weblogs.asp.net/dixin") };
            ref Uri firstReference = ref FirstReferenceByAlias(references); // Alias of references[0].
            firstReference = new Uri("https://flickr.com/dixin");
            references[0].WriteLine(); // https://flickr.com/dixin
        }

        internal static ref readonly int FirstValueByImmutableAlias(int[] values)
        {
            return ref values[0];
        }

        internal static ref readonly Uri FirstReferenceByImmutableAlias(Uri[] references)
        {
            return ref references[0];
        }

#if DEMO
        internal static void OutputByImmutableAlias()
        {
            int[] values = new int[] { 0, 1, 2, 3, 4 };
            ref readonly int firstValue = ref FirstValueByImmutableAlias(values); // Immutable alias of values[0].
            firstValue = 10; // Cannot be compiled.

            Uri[] references = new Uri[] { new Uri("https://weblogs.asp.net/dixin") };
            ref readonly Uri firstReference = ref FirstReferenceByImmutableAlias(references); // Immutable alias of references[0].
            firstReference = new Uri("https://flickr.com/dixin"); // Cannot be compiled.
        }
#endif
    }
}
