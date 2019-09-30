using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncBox
{
    public static class Program
    {
        public static void Main()
        {
            //AsyncLockingProblem();
            UpdatingList();
        }

        private static void UpdatingList()
        {
            var list = new List<object>();
            var source = new CancellationTokenSource();

            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();
            PrintChanges();


            AddObjects();

            Console.ReadKey();

            source.Cancel();

            async Task PrintChanges()
            {
                while (!source.IsCancellationRequested)
                {
                    await Task.Delay(10);
                    Console.WriteLine("Thread[{0}] Listener -> [{2}]", Thread.CurrentThread.ManagedThreadId, Task.CurrentId, list.Count);
                }
            }

            async Task AddObjects()
            {
                while (!source.IsCancellationRequested)
                {
                    list.Add(new object());
                    Console.WriteLine("--------------------------------Thread[{0}] Publisher -> [{2}]", Thread.CurrentThread.ManagedThreadId, Task.CurrentId, list.Count);
                    await Task.Delay(100);
                }
            }
        }

        private static void AsyncLockingProblem()
        {
            var _lock = new object();

            async Task LockMeUp()
            {
                Console.WriteLine("Lock me up {0}", Thread.CurrentThread.ManagedThreadId);
                await Task.Delay(100);
                Console.WriteLine("Lock me up after delay {0}", Thread.CurrentThread.ManagedThreadId);

                Monitor.Enter(_lock);
                await Task.Delay(400);
                Console.WriteLine("Lock me up [locked] {0} ", Thread.CurrentThread.ManagedThreadId);
                Monitor.Exit(_lock);
                Console.WriteLine("Lock me up [un-locked] {0} ", Thread.CurrentThread.ManagedThreadId);
            }

            LockMeUp();
            LockMeUp();

            Console.ReadKey();
        }

    }
}
