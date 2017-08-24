using System;
using System.Collections.Generic;
using System.Linq;

namespace TailRecursion.NET
{
    public class TailRecursion<TResult>
    {
        private TailRecursionContext<TResult> Context { get; }
        private Func<TailRecursionContext<TResult>, TResult> Implementation { get; }
        private Dictionary<string, Type> ArgumentDefinitions { get; }

        private bool IsSelfCalled { get; set; }
        private TResult TemporaryResult { get; set; }
        private object[] CurrentArguments { get; set; }

        public TailRecursion(Func<TailRecursionContext<TResult>, TResult> implementation, Dictionary<string, Type> argumentDefinitions)
        {
            Context = new TailRecursionContext<TResult>(Self);
            ArgumentDefinitions = argumentDefinitions;
            Implementation = implementation;
        }

        public TResult Run(params object[] arguments)
        {
            IsSelfCalled = true;
            CurrentArguments = arguments;
            while (IsSelfCalled)
            {
                foreach (var arg in ArgumentDefinitions.Zip(CurrentArguments, (pair, o) => (Type: pair.Value, Value: o, Name: pair.Key)))
                {
                    Context.Set(arg.Name, Convert.ChangeType(arg.Value, arg.Type));
                }

                IsSelfCalled = false;
                TemporaryResult = Implementation.Invoke(Context);
            }
            return TemporaryResult;
        }

        private TResult Self(params object[] arguments)
        {
            IsSelfCalled = true;
            CurrentArguments = arguments;
            return TemporaryResult;
        }
    }
}
