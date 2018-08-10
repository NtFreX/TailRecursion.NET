using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TailRecursion.NET.Generics
{
    public class RecursionFunc<TResult> : RecursionFuncBase<TResult>
    {
        private readonly RecursionContext<Func<Task<TResult>>> _context;
        private readonly Func<RecursionContext<Func<Task<TResult>>>, Task<TResult>> _fnc;

        public RecursionFunc(Func<RecursionContext<Func<Task<TResult>>>, Task<TResult>> fnc)
        {
            _context = new RecursionContext<Func<Task<TResult>>>(Self);
            _fnc = fnc;
        }

        public Func<Task<TResult>> Compile()
        {
            return new Func<Task<TResult>>(() =>
            {
                return Run(new object[] { });
            });
        }

        protected override Task<TResult> Invoke(object[] args)
            => _fnc.Invoke(_context);

        private Task<TResult> Self()
            => base.Self();
    }
    public class RecursionFunc<T, TResult> : RecursionFuncBase<TResult>
    {
        private readonly RecursionContext<Func<T, Task<TResult>>> _context;
        private readonly Func<RecursionContext<Func<T, Task<TResult>>>, T, Task<TResult>> _fnc;

        public RecursionFunc(Func<RecursionContext<Func<T, Task<TResult>>>, T, Task<TResult>> fnc)
        {
            _context = new RecursionContext<Func<T, Task<TResult>>>(Self);
            _fnc = fnc;
        }

        public Func<T, Task<TResult>> Compile()
        {
            return new Func<T, Task<TResult>>((arg) =>
            {
                return Run(new object[] { arg });
            });
        }

        protected override Task<TResult> Invoke(object[] args)
            => _fnc.Invoke(_context, (T)args[0]);

        private Task<TResult> Self(T arg)
            => base.Self(arg);
    }
    public class RecursionFunc<T, T1, TResult> : RecursionFuncBase<TResult>
    {
        private readonly RecursionContext<Func<T, T1, Task<TResult>>> _context;
        private readonly Func<RecursionContext<Func<T, T1, Task<TResult>>>, T, T1, Task<TResult>> _fnc;

        public RecursionFunc(Func<RecursionContext<Func<T, T1, Task<TResult>>>, T, T1, Task<TResult>> fnc)
        {
            _context = new RecursionContext<Func<T, T1, Task<TResult>>>(Self);
            _fnc = fnc;
        }

        public Func<T, T1, Task<TResult>> Compile()
        {
            return new Func<T, T1, Task<TResult>>((arg, arg1) =>
            {
                return Run(new object[] { arg, arg1 });
            });
        }

        protected override Task<TResult> Invoke(object[] args)
            => _fnc.Invoke(_context, (T)args[0], (T1)args[1]);

        private Task<TResult> Self(T arg, T1 arg1)
            => base.Self(arg, arg1);
    }

    //TODO: remove base class to get rid of casting/boxing invoke? or make wrapper
    public abstract class RecursionFuncBase<TResult>
    {
        protected readonly AutoResetEvent ActionEvent;
        protected readonly Stack<TaskCompletionSource<TResult>> ResultStack;
        protected readonly Stack<Task<TResult>> TaskStack;

        private object[] _args;

        public RecursionFuncBase()
        {
            ResultStack = new Stack<TaskCompletionSource<TResult>>();
            TaskStack = new Stack<Task<TResult>>();
            ActionEvent = new AutoResetEvent(true);
        }

        protected abstract Task<TResult> Invoke(object[] args);

        protected Task<TResult> Run(object[] args)
        {
            _args = args;

            while (true)
            {
                if (ActionEvent.WaitOne(0))
                {
                    var task = Invoke(_args);
                    TaskStack.Push(task);
                }
                else
                {
                    var nextTask = TaskStack.Peek();
                    if (nextTask.IsCompleted)
                    {
                        var result = TaskStack.Pop().GetAwaiter().GetResult();
                        if (!ResultStack.Any())
                        {
                            return Task.FromResult(result);
                        }
                        else
                        {
                            ResultStack.Pop().SetResult(result);
                        }
                    }
                    else if (nextTask.IsCanceled)
                    {
                        throw new TaskCanceledException();
                    }
                    else if (nextTask.IsFaulted)
                    {
                        if(nextTask.Exception.InnerExceptions.Count() > 1)
                        {
                            throw nextTask.Exception;
                        }
                        else
                        {
                            throw nextTask.Exception.InnerExceptions.First();
                        }
                    }
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
