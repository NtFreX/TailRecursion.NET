using System;

namespace TailRecursion.NET.Demo
{
    // TODO: implement generic versions of `TailRecursion` to get rid of the `Get` and `Set` methods on the context and the `ArgumentDefinitions`
    public class Program
    {
        public static void Main(string[] args)
        {
            uint input = 123212;

            Console.WriteLine("Tail Recursion Optimizion!");

            var facOptimized = new TailRecursionBuilder()
                .AddArgument<uint>("acc")
                .AddArgument<uint>("n")
                .Build<uint>(context =>
                {
                    var n = context.Get<uint>("n");
                    var acc = context.Get<uint>("acc");

                    if (n < 2) return acc;
                    return context.Self.Invoke(new object[] {n * acc, n - 1});
                });

            // => no stackoverflow exception expected
            Console.WriteLine(facOptimized.Run(1, input));
            // => stackoverflow exception expected
            Console.WriteLine(Fac(input));

            Console.ReadLine();
        }

        private static uint Fac(uint n)
        {
            if (n < 2) return 1;
            uint acc = Fac(n - 1);
            return n * acc;
        }
    }
}
