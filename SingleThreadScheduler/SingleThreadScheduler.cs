using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SingleThreadScheduler
{
    /// <summary>
    /// Implementation of <see cref="TaskScheduler"/> that runs tasks on a single, dedicated thread.
    /// </summary>
    internal class SingleThreadScheduler : TaskScheduler, IDisposable
    {
        private BlockingCollection<Task> scheduledTasks = new BlockingCollection<Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public SingleThreadScheduler()
        {
            this.StartWork();
        }

        private void StartWork()
        {
            Task.Factory.StartNew(() =>
            {
                for (; ; )
                {
                    if (scheduledTasks.IsAddingCompleted)
                    {
                        break;
                    }

                    var task = scheduledTasks.Take(this.cancellationTokenSource.Token);
                    base.TryExecuteTask(task);
                }
            },
            this.cancellationTokenSource.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return scheduledTasks.AsEnumerable();
        }

        protected override void QueueTask(Task task)
        {
            this.scheduledTasks.Add(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // Tasks cannot be executed inline.
            return false;
        }

        public void Dispose()
        {
            scheduledTasks.CompleteAdding();
            cancellationTokenSource.Cancel();
            //((IDisposable)this.cancellationTokenSource).Dispose();
            //((IDisposable)this.scheduledTasks).Dispose();
        }
    }
}