namespace TailRecursion.NET.Generics
{
    public class TailRecursionContext<T>
    {
        public T Self { get; }

        public TailRecursionContext(T self)
        {
            Self = self;
        }
    }
}
