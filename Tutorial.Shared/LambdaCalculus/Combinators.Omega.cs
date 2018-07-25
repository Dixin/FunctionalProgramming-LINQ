namespace Tutorial.LambdaCalculus
{
    using System.Diagnostics.CodeAnalysis;

#if DEMO
    public delegate TResult Func<TResult>(?);

    public delegate TResult Func<TResult>(Func<TResult> self);
#endif

    public delegate TResult SelfApplicableFunc<TResult>(SelfApplicableFunc<TResult> self);

    [SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase")]
    public static class OmegaCombinators<TResult>
    {
        public static readonly SelfApplicableFunc<TResult>
#pragma warning disable SA1307 // Accessible fields must begin with upper-case letter
#pragma warning disable SA1311 // Static readonly fields must begin with upper-case letter
            ω = f => f(f);
#pragma warning restore SA1311 // Static readonly fields must begin with upper-case letter
#pragma warning restore SA1307 // Accessible fields must begin with upper-case letter

        public static readonly TResult
            Ω = ω(ω);
    }
}