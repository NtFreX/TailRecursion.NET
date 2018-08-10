using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TailRecursion.NET.Generics
{
    public class TailRecursionAction : TailRecursionActionBase
    {
        private readonly TailRecursionContext<Action> _context;

        public TailRecursionAction(Action<TailRecursionContext<Action>> fnc)
            : base(fnc)
        {
            _context = new TailRecursionContext<Action>(Self);
        }

        public Func<Task> Compile()
        {
            return new Func<Task>(() =>
            {
                return Run(new object[] { _context });
            });
        }

        private void Self()
            => Self(_context);
    }

    public class TailRecursionAction<T> : TailRecursionActionBase
    {
        private readonly TailRecursionContext<Action<T>> _context;
        
        public TailRecursionAction(Action<TailRecursionContext<Action<T>>, T> fnc)
            : base(fnc)
        {
            _context = new TailRecursionContext<Action<T>>(Self);
        }

        public Func<T, Task> Compile()
        {
            return new Func<T, Task>(arg =>
            {
                return Run(new object[] { _context, arg });
            });
        }

        private void Self(T arg)
            => Self(_context, arg);
    }

    public class TailRecursionAction<T, T1> : TailRecursionActionBase
    {
        private readonly TailRecursionContext<Action<T, T1>> _context;
        
        public TailRecursionAction(Action<TailRecursionContext<Action<T, T1>>, T, T1> fnc)
            : base(fnc)
        {
            _context = new TailRecursionContext<Action<T, T1>>(Self);
        }

        public Func<T, T1, Task> Compile()
        {
            return new Func<T, T1, Task>((arg, arg1) =>
            {
                return Run(new object[] { _context, arg, arg1 });
            });
        }

        private void Self(T arg, T1 arg1)
            => Self(_context, arg, arg1);
    }

    //TODO: remove base class to get rid of dynamic invoke?
    //TODO: task compleation source without result...
    public abstract class TailRecursionActionBase
    {
        protected readonly Delegate Fnc;

        protected readonly AutoResetEvent ActionEvent;
        protected readonly Stack<TaskCompletionSource<object>> ResultStack;
        protected readonly Stack<Task> TaskStack;

        private object[] _args;

        public TailRecursionActionBase(Delegate fnc)
        {
            ResultStack = new Stack<TaskCompletionSource<object>>();
            TaskStack = new Stack<Task>();
            ActionEvent = new AutoResetEvent(true);

            Fnc = fnc;
        }

        protected Task Run(object[] args)
        {
            _args = args;

            while (true)
            {
                if (ActionEvent.WaitOne(0))
                {
                    var task = (Task)Fnc.DynamicInvoke(_args);
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
