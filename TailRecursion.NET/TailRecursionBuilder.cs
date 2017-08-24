using System;
using System.Collections.Generic;

namespace TailRecursion.NET
{
    public class TailRecursionBuilder
    {
        private readonly Dictionary<string, Type> _arguments = new Dictionary<string, Type>();

        public TailRecursionBuilder AddArgument<TArg>(string name)
        {
            _arguments.Add(name, typeof(TArg));
            return this;
        }

        public TailRecursion<TResult> Build<TResult>(Func<TailRecursionContext<TResult>, TResult> func)
        {
            return new TailRecursion<TResult>(func, _arguments);
        }
    }
}
