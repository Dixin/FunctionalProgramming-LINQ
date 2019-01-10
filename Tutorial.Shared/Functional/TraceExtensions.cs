namespace Tutorial
{
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
}
