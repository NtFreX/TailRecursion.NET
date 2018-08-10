using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TailRecursion.NET.Generics
{
    public class TailRecursionFunc<TResult> : TailRecursionFuncBase<TResult>
    {
        private readonly TailRecursionContext<Func<Task<TResult>>> _context;
        
        public TailRecursionFunc(Func<TailRecursionContext<Func<Task<TResult>>>, Task<TResult>> fnc)
            : base(fnc)
        {
            _context = new TailRecursionContext<Func<Task<TResult>>>(Self);
        }

        public Func<Task<TResult>> Compile()
        {
            return new Func<Task<TResult>>(() =>
            {
                return Run(new object[] { _context });
            });
        }

        private Task<TResult> Self()
            => Self(_context);
    }
    public class TailRecursionFunc<T, TResult> : TailRecursionFuncBase<TResult>
    {
        private readonly TailRecursionContext<Func<T, Task<TResult>>> _context;

        public TailRecursionFunc(Func<TailRecursionContext<Func<T, Task<TResult>>>, T, Task<TResult>> fnc)
            : base(fnc)
        {
            _context = new TailRecursionContext<Func<T, Task<TResult>>>(Self);
        }

        public Func<T, Task<TResult>> Compile()
        {
            return new Func<T, Task<TResult>>((arg) =>
            {
                return Run(new object[] { _context, arg });
            });
        }

        private Task<TResult> Self(T arg)
            => Self(_context, arg);
    }
    public class TailRecursionFunc<T, T1, TResult> : TailRecursionFuncBase<TResult>
    {
        private readonly TailRecursionContext<Func<T, T1, Task<TResult>>> _context;
        
        public TailRecursionFunc(Func<TailRecursionContext<Func<T, T1, Task<TResult>>>, T, T1, Task<TResult>> fnc)
            : base(fnc)
        {
            _context = new TailRecursionContext<Func<T, T1, Task<TResult>>>(Self);
        }

        public Func<T, T1, Task<TResult>> Compile()
        {
            return new Func<T, T1, Task<TResult>>((arg, arg1) =>
            {
                return Run(new object[] { _context, arg, arg1 });
            });
        }

        private Task<TResult> Self(T arg, T1 arg1)
            => Self(_context, arg, arg1);
    }

    //TODO: remove base class to get rid of dynamic invoke? or make wrapper
    public abstract class TailRecursionFuncBase<TResult>
    {
        protected readonly Delegate Fnc;

        protected readonly AutoResetEvent ActionEvent;
        protected readonly Stack<TaskCompletionSource<TResult>> ResultStack;
        protected readonly Stack<Task<TResult>> TaskStack;

        private object[] _args;

        public TailRecursionFuncBase(Delegate fnc)
        {
            ResultStack = new Stack<TaskCompletionSource<TResult>>();
            TaskStack = new Stack<Task<TResult>>();
            ActionEvent = new AutoResetEvent(true);

            Fnc = fnc;
        }

        protected Task<TResult> Run(object[] args)
        {
            _args = args;

            while (true)
            {
                if (ActionEvent.WaitOne(0))
                {
                    var task = (Task<TResult>) Fnc.DynamicInvoke(_args);
                    TaskStack.Push(task);
                }
                else if (TaskStack.Peek().IsCompleted)
                {
                    var result = TaskStack.Pop().GetAwaiter().GetResult();
                    if (ResultStack.Count == 0)
                    {
                        return Task.FromResult(result);
                    }
                    else
                    {
                        ResultStack.Pop().SetResult(result);
                    }
                }
                else if (TaskStack.Peek().IsCanceled)
                {
                    throw new Exception();
                }
                else if (TaskStack.Peek().IsFaulted)
                {
                    throw new Exception();
                }
            }
        }

        protected Task<TResult> Self(params object[] args)
        {
            var tcs = new TaskCompletionSource<TResult>();
            ResultStack.Push(tcs);

            _args = args;

            ActionEvent.Set();

            return tcs.Task;
        }
    }
}
