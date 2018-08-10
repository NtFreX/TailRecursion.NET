# TailRecursion.NET

The following code will throw an stackoverflow exception.

    private static uint Fac(uint n)
    {
        if (n < 2) return 1;
        uint acc = Fac(n - 1);
        return n * acc;
    }
    
    Fac(123212);

**Tail recursion optimized:**

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
            
    Console.WriteLine(facOptimized.Run(1, 123212));

**Recursion optimized:**
       
    var facOptimizedGeneric = new Generics.TailRecursionFunc<uint, uint>(async (context, n) =>
    {
        if (n < 2) return 1;
        uint acc = await context.Self(n - 1);
        return n * acc;
    }).Compile();
    
    Console.WriteLine(await facOptimizedGeneric(123212));