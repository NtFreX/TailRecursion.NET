using System;
using System.Diagnostics;
using TailRecursion.NET.Generics;

namespace TailRecursion.NET.Demo
{
    public class Program
    {
        //TODO: real benchmarks
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

            var facOptimizedGeneric = new RecursionFunc<uint, uint>(async (context, n) =>
            {
                if (n < 2) return 1;
                uint acc = await context.Self(n - 1);
                return n * acc;
            }).Compile();

            Func<uint, uint> nonOptimizd = null;
            nonOptimizd = new Func<uint, uint>((uint n) =>
            {
                if (n < 2) return 1;
                uint acc = nonOptimizd(n - 1);
                return n * acc;
            });

            // => no stackoverflow exception expected
            var time = Measure("Optimized", () => facOptimized.Run(1, input));
            // => no stackoverflow exception expected
            var timeGeneric = Measure("Generic Optimized", () => facOptimizedGeneric(input).GetAwaiter().GetResult());
            // => stackoverflow exception expected
            //Measure("Non Optimized", () => nonOptimizd(input));

            Console.WriteLine($"{timeGeneric.TotalSeconds / time.TotalSeconds}");

            Console.ReadLine();
        }

        private static TimeSpan Measure<T>(string name, Func<T> action)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = action();
            var time = stopWatch.Elapsed;

            Console.WriteLine($"{name}: {result} in {time.TotalSeconds}s");

            return time;
        }
    }
}
