namespace TailRecursion.NET.Generics
{
    public class RecursionContext<T>
    {
        public T Self { get; }

        public RecursionContext(T self)
        {
            Self = self;
        }
    }
}
