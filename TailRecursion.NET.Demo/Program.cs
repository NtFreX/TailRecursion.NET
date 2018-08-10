using System;
using System.Threading.Tasks;

namespace TailRecursion.NET.Demo
{
    public class Program
    {
        //TODO: dynamic invoke makes this very slow
        //TODO: benchmarks
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
                    return context.Self.Invoke(new object[] { n * acc, n - 1 });
                });

            var facOptimizedGeneric = new Generics.TailRecursionFunc<uint, uint>(FacOptimized).Compile();

            // => no stackoverflow exception expected
            Console.WriteLine(facOptimized.Run(1, input));
            // => no stackoverflow exception expected
            Console.WriteLine(facOptimizedGeneric(input).GetAwaiter().GetResult());
            // => stackoverflow exception expected
            Console.WriteLine(Fac(input));

            Console.ReadLine();
        }

        private static async Task<uint> FacOptimized(Generics.TailRecursionContext<Func<uint, Task<uint>>> context, uint n)
        {
            if (n < 2) return 1;
            uint acc = await context.Self(n - 1);
            return n * acc;
        }

        private static uint Fac(uint n)
        {
            if (n < 2) return 1;
            uint acc = Fac(n - 1);
            return n * acc;
        }
    }
}
