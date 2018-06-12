namespace Tutorial.Functional
{
    using System;

    internal static partial class FirstClassFunctions
    {
        internal static void CurryFunc<T1, T2, T3, TN, TResult>()
        {
            // (T1, T2, T3, T4, ... TN) -> TResult
            Func<T1, T2, T3, /* T4, ... */ TN, TResult> function =
                (value1, value2, value3, /* value4, ... */ valueN) => default;
            // T1 -> T2 -> T3 -> ... TN -> TResult
            Func<T1, Func<T2, Func<T3, /* Func<T4, ... */ Func<TN, TResult>/* ...> */>>> curriedFunction =
                value1 => value2 => value3 => /* value4 => ... */ valueN => default;
        }

        // Transform (T1, T2) -> TResult
        // to T1 -> T2 -> TResult.
        public static Func<T1, Func<T2, TResult>> Curry<T1, T2, TResult>(
            this Func<T1, T2, TResult> function) =>
                value1 => value2 => function(value1, value2);

        // Transform (T1, T2, T3) -> TResult
        // to T1 -> T2 -> T3 -> TResult.
        public static Func<T1, Func<T2, Func<T3, TResult>>> Curry<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> function) =>
                value1 => value2 => value3 => function(value1, value2, value3);

        // Transform (T1, T2, T3, T4) => TResult
        // to T1 -> T2 -> T3 -> T4 -> TResult.
        public static Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>> Curry<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> function) =>
                value1 => value2 => value3 => value4 => function(value1, value2, value3, value4);

        // ...

        internal static void CurryFunction()
        {
            // (int, int) -> int
            Func<int, int, int> add2Args = (a, b) => a + b;
            int add2ArgsResult = add2Args(1, 2);

            // int -> int -> int
            Func<int, Func<int, int>> curriedAdd2Args = add2Args.Curry();
            int curriedAdd2ArgsResult = curriedAdd2Args(1)(2);

            // (int, int, int) -> int
            Func<int, int, int, int> add3Args = (a, b, c) => a + b + c;
            int add3ArgsResult = add2Args(1, 2);

            // int -> int -> int -> int
            Func<int, Func<int, Func<int, int>>> curriedAdd3Args = add3Args.Curry();
            int curriedAdd3ArgsResult = curriedAdd3Args(1)(2)(3);
        }

        internal static void CurryAction<T1, T2, T3, TN>()
        {
            // (T1, T2, T3, ... TN) -> void
            Action<T1, T2, T3, /* T4, ... */ TN> function =
                (value1, value2, value3, /* value4, ... */ valueN) => { };
            // T1 -> T2 -> T3 -> ... TN -> void
            Func<T1, Func<T2, Func<T3, /* Func<T4, ... */ Action<TN>/* ...> */>>> curriedFunction =
                value1 => value2 => value3 => /* value4 => ... */ valueN => { };
        }

        // Transform (T1, T2) -> void
        // to T1 => T2 -> void.
        public static Func<T1, Action<T2>> Curry<T1, T2>(
            this Action<T1, T2> function) =>
            value1 => value2 => function(value1, value2);

        // Transform (T1, T2, T3) -> void
        // to T1 -> T2 -> T3 -> void.
        public static Func<T1, Func<T2, Action<T3>>> Curry<T1, T2, T3>(
            this Action<T1, T2, T3> function) =>
            value1 => value2 => value3 => function(value1, value2, value3);

        // Transform (T1, T2, T3, T4) -> void
        // to T1 -> T2 -> T3 -> T4 -> void.
        public static Func<T1, Func<T2, Func<T3, Action<T4>>>> Curry<T1, T2, T3, T4>(
            this Action<T1, T2, T3, T4> function) =>
            value1 => value2 => value3 => value4 => function(value1, value2, value3, value4);

        // ...

        internal static void CurryAction()
        {
            // (int, int) -> void
            Action<int, int> add2Args = (a, b) => (a + b).WriteLine();
            add2Args(1, 2);

            // int -> int -> void
            Func<int, Action<int>> curriedAdd2Args = add2Args.Curry();
            curriedAdd2Args(1)(2);

            // (int, int, int) -> void
            Action<int, int, int> add3Args = (a, b, c) => (a + b + c).WriteLine();
            add2Args(1, 2);

            // int -> int -> int -> void
            Func<int, Func<int, Action<int>>> curriedAdd3Args = add3Args.Curry();
            curriedAdd3Args(1)(2)(3);
        }

        // Transform T1 -> T2 -> TResult
        // to (T1, T2) -> TResult.
        public static Func<T1, T2, TResult> Uncurry<T1, T2, TResult>(
            this Func<T1, Func<T2, TResult>> function) =>
            (value1, value2) => function(value1)(value2);

        // Transform T1 -> T2 -> T3 -> TResult
        // to (T1, T2, T3) -> TResult.
        public static Func<T1, T2, T3, TResult> Uncurry<T1, T2, T3, TResult>(
            this Func<T1, Func<T2, Func<T3, TResult>>> function) =>
            (value1, value2, value3) => function(value1)(value2)(value3);

        // Transform T1 -> T2 -> T3 -> T4 -> TResult
        // to (T1, T2, T3, T4) -> TResult.
        public static Func<T1, T2, T3, T4, TResult> Uncurry<T1, T2, T3, T4, TResult>(
            this Func<T1, Func<T2, Func<T3, Func<T4, TResult>>>> function) =>
            (value1, value2, value3, value4) => function(value1)(value2)(value3)(value4);

        // ...

        // Transform T1 -> T2 -> void
        // to (T1, T2) -> void.
        public static Action<T1, T2> Uncurry<T1, T2>(
            this Func<T1, Action<T2>> function) => (value1, value2) =>
            function(value1)(value2);

        // Transform T1 -> T2 -> T3 -> void
        // to (T1, T2, T3) -> void.
        public static Action<T1, T2, T3> Uncurry<T1, T2, T3>(
            this Func<T1, Func<T2, Action<T3>>> function) =>
            (value1, value2, value3) => function(value1)(value2)(value3);

        // Transform T1 -> T2 -> T3 -> T4 -> void
        // to (T1, T2, T3, T4) -> void.
        public static Action<T1, T2, T3, T4> Uncurry<T1, T2, T3, T4>(
            this Func<T1, Func<T2, Func<T3, Action<T4>>>> function) =>
            (value1, value2, value3, value4) => function(value1)(value2)(value3)(value4);

        // ...

        internal static void Uncurry()
        {
            // int -> int -> int
            Func<int, Func<int, int>> curriedAdd2Args = a => b => a + b;
            // (int -> int) -> int
            Func<int, int, int> add2Args = curriedAdd2Args.Uncurry();
            int add2ArgsResult = add2Args(1, 2);

            // int -> int -> int -> void
            Func<int, Func<int, Action<int>>> curriedAdd3Args = a => b => c => (a + b + c).WriteLine();
            // (int -> int -> int) -> void
            Action<int, int, int> add3Args = curriedAdd3Args.Uncurry();
            add3Args(1, 2, 3);
        }

        internal static void FirstOrderToFirstOrder()
        {
            // (int, int) -> int
            Func<int, int, int> add2Args = (a, b) => a + b;

            // int -> int
            Func<int, int> add1ArgAnd1Constant = b => 1 + b; // Partially apply add2Args with a = 1.

            // (int, int, int) -> void
            Action<int, int, int> add3Args = (a, b, c) => (a + b + c).WriteLine();
            add2Args(1, 2);

            // (int, int) -> void
            Action<int, int> add2ArgsAnd1Constant = (b, c) => (1 + b + c).WriteLine();  // Partially apply add2Args with a = 1.
            // add2ArgsAnd1Constant can be called with 2 arguments, or be partially applied again with b = 2:
            // int -> void
            Action<int> add1ArgsAnd2Constant = c => (1 + 2 + c).WriteLine();
        }

        // Input: function of type (T1, T2) -> TResult, first parameter of type T1.
        // Output: function of type T2 -> TResult (with 1 less parameter).
        public static Func<T2, TResult> Partial<T1, T2, TResult>(
            this Func<T1, T2, TResult> function, T1 value1) =>
            value2 => function(value1, value2);

        // Input: function of type (T1, T2, T3) -> TResult, first parameter of type T1.
        // Output: function of type (T2, T3) -> TResult (with 1 less parameter).
        public static Func<T2, T3, TResult> Partial<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> function, T1 value1) =>
            (value2, value3) => function(value1, value2, value3);

        // Input: function of type (T1, T2, T3, T4) -> TResult, first parameter of type T1.
        // Output: function of type (T2, T3, T4) -> TResult (with 1 less parameter).
        public static Func<T2, T3, T4, TResult> Partial<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> function, T1 value1) =>
            (value2, value3, value4) => function(value1, value2, value3, value4);

        // ...

        // Input: function of type (T1, T2) -> void, first parameter of type T1.
        // Output: function of type T2 -> void (with 1 less parameter).
        public static Action<T2> Partial<T1, T2>(
            this Action<T1, T2> function, T1 value1) =>
            value2 => function(value1, value2);

        // Input: function of type (T1, T2, T3) -> void, first parameter of type T1.
        // Output: function of type (T2, T3) -> void (with 1 less parameter).
        public static Action<T2, T3> Partial<T1, T2, T3>(
            this Action<T1, T2, T3> function, T1 value1) =>
            (value2, value3) => function(value1, value2, value3);

        // Input: function of type (T1, T2, T3, T4) -> void, first parameter of type T1.
        // Output: function of type (T2, T3, T4) -> void (with 1 less parameter).
        public static Action<T2, T3, T4> Partial<T1, T2, T3, T4>(
            this Action<T1, T2, T3, T4> function, T1 value1) =>
            (value2, value3, value4) => function(value1, value2, value3, value4);

        // ...

        internal static void PartiallyApplyFirstOrderFunction()
        {
            // (int, int) -> int
            Func<int, int, int> add2Args = (a, b) => a + b;
            // int -> int
            Func<int, int> partiallyAppliedAdd2Args = add2Args.Partial(1);
            int partiallyAppliedAdd2ArgsResult = partiallyAppliedAdd2Args(2);

            // (int, int, int) -> void
            Action<int, int, int> add3Args = (a, b, c) => (a + b + c).WriteLine();
            // (int, int) -> void
            Action<int, int> partiallyAppliedAdd3Args = add3Args.Partial(1);
            partiallyAppliedAdd3Args(2, 3);
        }

        internal static void PartiallyApplyCurriedFunction()
        {
            // int -> int -> int
            Func<int, Func<int, int>> curriedAdd2Args = a => b => a + b;
            // int -> int
            Func<int, int> partiallyAppliedCurriedAdd2Args = curriedAdd2Args(1);

            // int -> int -> int -> void
            Func<int, Func<int, Action<int>>> curriedAdd3Args = a => b => c => (a + b + c).WriteLine();
            // int -> void
            Action<int> partiallyAppliedCurriedAdd3Args = curriedAdd3Args(1)(2);

        }
    }
}
