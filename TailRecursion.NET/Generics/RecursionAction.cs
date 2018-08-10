using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TailRecursion.NET.Generics
{
    public class RecursionAction : RecursionActionBase
    {
        private readonly RecursionContext<Func<Task>> _context;
        private readonly Func<RecursionContext<Func<Task>>, Task> _fnc;

        public RecursionAction(Func<RecursionContext<Func<Task>>, Task> fnc)
        {
            _context = new RecursionContext<Func<Task>>(Self);
            _fnc = fnc;
        }

        public Func<Task> Compile()
        {
            return new Func<Task>(() =>
            {
                return Run(new object[] { });
            });
        }

        protected override Task Invoke(object[] args)
            => _fnc.Invoke(_context);

        private Task Self()
            => base.Self();
    }

    public class RecursionAction<T> : RecursionActionBase
    {
        private readonly RecursionContext<Func<T, Task>> _context;
        private readonly Func<RecursionContext<Func<T, Task>>, T, Task> _fnc;

        public RecursionAction(Func<RecursionContext<Func<T, Task>>, T, Task> fnc)
        {
            _context = new RecursionContext<Func<T, Task>>(Self);
            _fnc = fnc;
        }

        public Func<T, Task> Compile()
        {
            return new Func<T, Task>(arg =>
            {
                return Run(new object[] { _context, arg });
            });
        }

        protected override Task Invoke(object[] args)
            => _fnc.Invoke(_context, (T)args[0]);

        private Task Self(T arg)
            => base.Self(arg);
    }

    public class RecursionAction<T, T1> : RecursionActionBase
    {
        private readonly RecursionContext<Func<T, T1, Task>> _context;
        private readonly Func<RecursionContext<Func<T, T1, Task>>, T, T1, Task> _fnc;

        public RecursionAction(Func<RecursionContext<Func<T, T1, Task>>, T, T1, Task> fnc)
        {
            _context = new RecursionContext<Func<T, T1, Task>>(Self);
            _fnc = fnc;
        }

        public Func<T, T1, Task> Compile()
        {
            return new Func<T, T1, Task>((arg, arg1) =>
            {
                return Run(new object[] { _context, arg, arg1 });
            });
        }

        protected override Task Invoke(object[] args)
            => _fnc.Invoke(_context, (T)args[0], (T1)args[1]);

        private Task Self(T arg, T1 arg1)
            => base.Self(arg, arg1);
    }

    //TODO: remove base class to get rid of casting/boxing invoke?
    //TODO: task compleation source without result is ugly but seems to way to go
    public abstract class RecursionActionBase
    {
        protected readonly AutoResetEvent ActionEvent;
        protected readonly Stack<TaskCompletionSource<object>> ResultStack;
        protected readonly Stack<Task> TaskStack;

        private object[] _args;

        public RecursionActionBase()
        {
            ResultStack = new Stack<TaskCompletionSource<object>>();
            TaskStack = new Stack<Task>();
            ActionEvent = new AutoResetEvent(true);
        }

        protected abstract Task Invoke(object[] args);

        protected Task Run(object[] args)
        {
            _args = args;

            while (true)
            {
                if (ActionEvent.WaitOne(0))
                {
                    var task = Invoke(_args);
                    TaskStack.Push(task);
                }
                else if (TaskStack.Peek().IsCompleted)
                {
                    TaskStack.Pop().GetAwaiter().GetResult();
                    if (ResultStack.Count == 0)
                    {
                        return Task.CompletedTask;
                    }
                    else
                    {
                        ResultStack.Pop().SetResult(null);
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

        protected Task Self(params object[] args)
        {
            var tcs = new TaskCompletionSource<object>();
            ResultStack.Push(tcs);

            _args = args;

            ActionEvent.Set();

            return tcs.Task;
        }
    }
}
