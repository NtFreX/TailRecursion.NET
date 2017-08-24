using System;
using System.Collections.Generic;

namespace TailRecursion.NET
{
    public class TailRecursionContext<TResult>
    {
        private readonly Dictionary<string, object> _arguments = new Dictionary<string, object>();

        public Func<object[], TResult> Self { get; }

        public TailRecursionContext(Func<object[], TResult> self)
        {
            Self = self;
        }

        public TArg Get<TArg>(string name)
        {
            if (_arguments.ContainsKey(name))
                return (TArg)_arguments[name];
            return default(TArg);
        }

        public void Set<TArg>(string name, TArg value)
        {
            if (!_arguments.ContainsKey(name))
                _arguments.Add(name, value);
            else
                _arguments[name] = value;
        }
    }
}
