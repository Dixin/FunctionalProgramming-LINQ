namespace Tutorial.LambdaCalculus
{
    using System;

    using static IotaCombinator;

    public static partial class IotaCombinator
    {
        public static readonly Func<dynamic, dynamic>
#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable SA1311 // Static readonly fields must begin with upper-case letter
            ι = f => f
#pragma warning restore SA1311 // Static readonly fields must begin with upper-case letter
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter
                (new Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>(x => y => z => x(z)(y(z)))) // S
                (new Func<dynamic, Func<dynamic, dynamic>>(x => y => x)); // K
    }

    public static class IotaCalculus
    {
        public static readonly Func<dynamic, Func<dynamic, Func<dynamic, dynamic>>>
            S = ι(ι(ι(ι(ι))));

        public static readonly Func<dynamic, Func<dynamic, dynamic>>
            K = ι(ι(ι(ι)));

        public static readonly Func<dynamic, dynamic>
            I = ι(ι);
    }
}
