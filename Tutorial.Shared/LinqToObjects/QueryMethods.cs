namespace Tutorial.LinqToObjects
{
    using System.Collections.Generic;

    internal static partial class QueryMethods
    {
        internal static void EmptyIfNull(IEnumerable<int> source1, IEnumerable<int> source2)
        {
            IEnumerable<int> positive = source1.EmptyIfNull()
                .Union(source2.EmptyIfNull())
                .Where(int32 => int32 > 0);
        }
    }
}
