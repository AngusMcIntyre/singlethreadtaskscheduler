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

        private object disposeLock = new object();

        public SingleThreadScheduler()
        {
            this.StartWork();
        }

        private void StartWork()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    lock (this.disposeLock)
                    {
                        for (; ; )
                        {
                            if (scheduledTasks.IsAddingCompleted
                            || this.cancellationTokenSource.IsCancellationRequested)
                            {
                                break;
                            }

                            if (scheduledTasks.TryTake(out var task, Timeout.Infinite, this.cancellationTokenSource.Token))
                            {
                                base.TryExecuteTask(task);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Swallow this exception as it is excepted
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

            lock (this.disposeLock)
            {
                ((IDisposable)this.cancellationTokenSource).Dispose();
                ((IDisposable)this.scheduledTasks).Dispose(); 
            }
        }
    }
}