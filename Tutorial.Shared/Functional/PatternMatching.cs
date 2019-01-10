namespace Tutorial.Functional
{
    using System;
    using System.IO;

    internal static partial class PatternMatching
    {
        internal static void IsTypePattern(object @object)
        {
            if (@object is Uri reference)
            {
                reference.AbsolutePath.WriteLine();
            }

            if (@object is DateTime value)
            {
                value.ToString("o").WriteLine();
            }
        }

        internal static void CompiledIsTypePattern(object @object)
        {
            Uri reference = @object as Uri;
            if (reference != null)
            {
                reference.AbsolutePath.WriteLine();
            }

            DateTime? nullableValue = @object as DateTime?;
            DateTime value = nullableValue.GetValueOrDefault();
            if (nullableValue.HasValue)
            {
                value.ToString("o").WriteLine();
            }
        }

        internal static void IsWithTest(object @object)
        {
            if (@object is string @string && TimeSpan.TryParse(@string, out TimeSpan timeSpan))
            {
                timeSpan.TotalMilliseconds.WriteLine();
            }
        }

        internal static void CompiledIsWithTest(object @object)
        {
            string @string = @object as string;
            if (@string != null && TimeSpan.TryParse(@string, out TimeSpan timeSpan))
            {
                timeSpan.TotalMilliseconds.WriteLine();
            }
        }

        internal static void IsWithOpenType<TOpen1, TOpen2, TOpen3, TOpen4>(
            IDisposable disposable, TOpen2 open2, TOpen3 open3)
        {
            if (disposable is TOpen1 open1)
            {
                open1.WriteLine();
            }

            if (open2 is FileInfo file)
            {
                file.WriteLine();
            }

            if (open3 is TOpen4 open4)
            {
                open4.WriteLine();
            }
        }

        internal static void CompiledIsWithOpenType<TOpen1, TOpen2, TOpen3, TOpen4>(
            IDisposable disposable, TOpen2 open2, TOpen3 open3)
        {
            object disposableObject = (object)disposable;
            if (disposableObject is TOpen1)
            {
                TOpen1 open1 = (TOpen1)disposableObject;
                open1.WriteLine();
            }

            object open2Object = (object)open2;
            FileInfo file = open2Object as FileInfo;
            if (file != null)
            {
                file.WriteLine();
            }

            object open3Object = (object)open3;
            if (open3Object is TOpen4)
            {
                TOpen4 open4 = (TOpen4)open3Object;
                open4.WriteLine();
            }
        }

        internal static void IsAnyType(object @object)
        {
            if (@object is var match)
            {
                object.ReferenceEquals(@object, match).WriteLine();
            }
        }

        internal static void CompiledIsAnyType(object @object)
        {
            object match = @object;
            if (true)
            {
                object.ReferenceEquals(@object, match).WriteLine();
            }
        }

        internal static void IsConstantPattern(object @object)
        {
            bool test1 = @object is null;
            bool test2 = @object is default(int);
            bool test3 = @object is DayOfWeek.Saturday - DayOfWeek.Monday;
            bool test4 = @object is "https://" + "flickr.com/dixin";
            bool test5 = @object is nameof(test5);
        }

        internal static void CompiledIsConstantPattern(object @object)
        {
            bool test1 = @object == null;
            bool test2 = object.Equals(0, @object);
            bool test3 = object.Equals(5, @object);
            bool test4 = object.Equals("https://flickr.com/dixin", @object);
            bool test5 = object.Equals("test5", @object);
        }

        internal static void IsConstantPatternWithDefault(object @object)
        {
#if DEMO
            // https://github.com/dotnet/roslyn/issues/25450
            // https://github.com/dotnet/roslyn/issues/23499
            bool test6 = @object is default; // Cannot be compiled. use default(Type).
#endif
        }

        internal static DateTime ToDateTime<TConvertible>(object @object)
            where TConvertible : IConvertible
        {
            switch (@object)
            {
                // Match null reference.
                case null:
                    throw new ArgumentNullException(nameof(@object));
                // Match value type.
                case DateTime dateTIme:
                    return dateTIme;
                // Match value type with condition.
                case long ticks when ticks >= 0:
                    return new DateTime(ticks);
                // Match reference type with condition.
                case string @string when DateTime.TryParse(@string, out DateTime dateTime):
                    return dateTime;
                // Match reference type with condition.
                case int[] date when date.Length == 3 && date[0] > 0 && date[1] > 0 && date[2] > 0:
                    return new DateTime(year: date[0], month: date[1], day: date[2]);
                // Match generics open type.
                case TConvertible convertible:
                    return convertible.ToDateTime(provider: null);
                // Match anything else. Equivalent to default case.
                case var _:
                    throw new ArgumentOutOfRangeException(nameof(@object));
            }
        }

        internal static DateTime CompiledToDateTime<TConvertible>(object @object)
            where TConvertible : IConvertible
        {
            // case null:
            if (@object == null)
            {
#pragma warning disable CA1507 // Use nameof to express symbol names
                throw new ArgumentNullException("object");
#pragma warning restore CA1507 // Use nameof to express symbol names
            }

            // case DateTime dateTIme:
            DateTime? nullableDateTime = @object as DateTime?;
            DateTime dateTime = nullableDateTime.GetValueOrDefault();
            if (nullableDateTime.HasValue)
            {
                return dateTime;
            }

            // case long ticks:
            long? nullableInt64 = @object as long?;
            long ticks = nullableInt64.GetValueOrDefault();
            if (nullableInt64.HasValue && ticks >= 0L) // when clause.
            {
                return new DateTime(ticks);
            }

            // case string text:
            string @string = @object as string;
            if (@string != null && DateTime.TryParse(@string, out DateTime parsedDateTime)) // when clause.
            {
                return parsedDateTime;
            }

            // case int[] date:
            int[] date = @object as int[];
            if (date != null && date.Length == 3 && date[0] >= 0 && date[1] >= 0 && date[2] >= 0) // when clause.
            {
                return new DateTime(date[0], date[1], date[2]);
            }

            // case TConvertible convertible:
            object convertibleObject = (object)@object;
            if (convertibleObject is TConvertible)
            {
                TConvertible convertible = (TConvertible)convertibleObject;
                return convertible.ToDateTime(null);
            }

            // case var _:
#pragma warning disable CA1507 // Use nameof to express symbol names
            throw new ArgumentOutOfRangeException("object");
#pragma warning restore CA1507 // Use nameof to express symbol names
        }
    }
}

#if DEMO
namespace System
{
    public class Object
    {
        public static bool Equals(object objA, object objB) =>
            objA == objB || (objA != null && objB != null && objA.Equals(objB));
    }
}

namespace System
{
    public readonly partial struct DateTime
    {
        public override bool Equals(object value)
        {
            if (value is DateTime)
            {
                return this.InternalTicks == ((DateTime)value).InternalTicks;
            }
            return false;
        }
    }

    public struct TimeSpan
    {
        public override bool Equals(object value)
        {
            if (value is TimeSpan)
            {
                return this._ticks == ((TimeSpan)value)._ticks;
            }
            return false;
        }
    }
}

namespace System
{
    public readonly partial struct DateTime
    {
        public override bool Equals(object value) => 
            value is DateTime dateTime && this.InternalTicks == dateTime.InternalTicks;
    }

    public struct TimeSpan
    {
        public override bool Equals(object value) => 
            value is TimeSpan timeSpan && this._ticks == timeSpan._ticks;
    }
}
#endif