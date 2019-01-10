namespace Tutorial
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static partial class TraceExtensions
    {
        public static void WriteLines<T>(this IEnumerable<T> values, Func<T, string> messageFactory = null)
        {
            if (messageFactory != null)
            {
                foreach (T value in values)
                {
                    Trace.WriteLine(messageFactory(value));
                }
            }
            else
            {
                foreach (T value in values)
                {
                    Trace.WriteLine(value);
                }
            }
        }
    }
}
