namespace Tutorial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class TraceExtensions
    {
        public static T WriteLine<T>(this T value)
        {
            Trace.WriteLine(value);
            return value;
        }

        public static T Write<T>(this T value)
        {
            Trace.Write(value);
            return value;
        }
    }

    public static partial class TraceExtensions
    {
        public static IEnumerable<T> WriteLines<T>(this IEnumerable<T> values, Func<T, string> messageSelector = null)
        {
            if (messageSelector != null)
            {
                foreach (T value in values)
                {
                    Trace.WriteLine(messageSelector(value));
                }
            }
            else
            {
                foreach (T value in values)
                {
                    Trace.WriteLine(value);
                }
            }
            return values;
        }
    }
}
