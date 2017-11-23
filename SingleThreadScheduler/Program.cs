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
                Task.WaitAll(GenerateTaskBatch(scheduler));
            }

            Task.WaitAll(GenerateTaskBatch(TaskScheduler.Default));

            Console.WriteLine("Done. Press ENTER to exit.");
            Console.ReadLine();
        }

        private static Task[] GenerateTaskBatch(TaskScheduler scheduler)
        {
            Console.WriteLine("Press ENTER to execute tasks on {0}.", scheduler.GetType().Name);
            Console.ReadLine();

            Task[] tasks = new Task[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                int actionNumber = i;
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine($"TID: {Thread.CurrentThread.ManagedThreadId}, Executing action {actionNumber + 1}");
                },
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                scheduler);
            }

            return tasks;
        }
    }
}
