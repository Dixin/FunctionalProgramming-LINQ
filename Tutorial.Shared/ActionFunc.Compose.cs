namespace Tutorial
{
    using System;

    public static partial class FuncExtensions
    {
        public static Func<T, TResult2> o<T, TResult1, TResult2>( // After.
            this Func<TResult1, TResult2> function2, Func<T, TResult1> function1) =>
                value => function2(function1(value));
    }
}
