namespace Tutorial.CategoryTheory
{
    using System;

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct Optional<T>
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        private readonly Lazy<(bool, T)> factory;

        public Optional(Func<(bool, T)> factory = null) =>
            this.factory = factory == null ? null : new Lazy<(bool, T)>(factory);

        public bool HasValue => this.factory?.Value.Item1 ?? false;

        public T Value
        {
            get
            {
                if (!this.HasValue)
                {
                    throw new InvalidOperationException($"{nameof(Optional<T>)} object must have a value.");
                }
                return this.factory.Value.Item2;
            }
        }
    }
}
