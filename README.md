# TailRecursion.NET

This library enables you to optimize tail recursion functions so no stackoverflow exception will be thrown.

The following code will throw an stackoverflow exception.

    private static uint Fac(uint n)
    {
        if (n < 2) return 1;
        uint acc = Fac(n - 1);
        return n * acc;
    }
    
    Fac(123212);

This library gives you two options to optimize this function.

**Non generic version**

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
       
**Generic version** 
       
    var facOptimizedGeneric = new Generics.TailRecursionFunc<uint, uint>(async (context, n) =>
    {
        if (n < 2) return 1;
        uint acc = await context.Self(n - 1);
        return n * acc;
    }).Compile();
    
    Console.WriteLine(await facOptimizedGeneric(123212));

*At the current state of this library the generic approach is a lot slower then the other because of the use of `DynamicInvoke`.*
