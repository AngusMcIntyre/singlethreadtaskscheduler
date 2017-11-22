using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingleThreadScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            using (SingleThreadScheduler scheduler = new SingleThreadScheduler())
            {
                var tasks = GenerateTaskBatch(scheduler);

                scheduler.Dispose();

                tasks = GenerateTaskBatch(scheduler);

                Task.WaitAll(tasks);

                tasks = GenerateTaskBatch(TaskScheduler.Default);

                Task.WaitAll(tasks);

                Console.WriteLine("Done. Press ENTER to exit.");
                Console.ReadLine();
            }
        }

        private static Task[] GenerateTaskBatch(TaskScheduler scheduler)
        {
            Console.WriteLine("Press ENTER to execute tasks on {0}.", scheduler.GetType().Name);
            Console.ReadLine();

            Task[] tasks = new Task[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                int actionNumber = i;
                tasks[i] = RunTaskOnScheduler(() =>
                {
                    Console.WriteLine($"TID: {Thread.CurrentThread.ManagedThreadId}, Executing action {actionNumber + 1}");
                }, 
                scheduler);
            }

            return tasks;
        }

        private static Task RunTaskOnScheduler(Action action, TaskScheduler scheduler)
        {
            TaskFactory factory = new TaskFactory(
                CancellationToken.None, 
                TaskCreationOptions.DenyChildAttach,
                TaskContinuationOptions.None, 
                scheduler);

            return factory.StartNew(action);
        }
    }
}
