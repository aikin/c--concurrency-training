using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace c.sharp.concurrency.training
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            //            TaskAndThread();

            //            ThreadState();

            //            setProcessorAffinity();

            //            Closures();

            //            FaultyThread();

            //            BadFaultyThread();

//            StartTaskWay();

//            TaskResultBlock();
            
//            ContinuationInnerTask();
            
            

            Console.WriteLine("Main methond complete. Press any key to finish.");
            Console.ReadKey();
        }

        private static void ContinuationInnerTask()
        {
            var firstTask = new Task<int>(() => TaskLogging("task 1", 3));
            var secondTask = new Task<int>(() => TaskLogging("task 2", 2));

            firstTask.ContinueWith(t =>
            {
                Console.WriteLine($"The first answer is {t.Result}.   " +
                                  $"First task is running on a thread id {Thread.CurrentThread.ManagedThreadId}.  " +
                                  $"Is thread poll thread: {Thread.CurrentThread.IsThreadPoolThread}");
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            firstTask.Start();
//            secondTask.Start();
//            Thread.Sleep(TimeSpan.FromSeconds(4));

            var continuation = secondTask.ContinueWith(t =>
            {
                Console.WriteLine($"The second answer is {t.Result}.  " +
                                  $"Second task is running on a thread id {Thread.CurrentThread.ManagedThreadId}.  " +
                                  $"Is thread poll thread: {Thread.CurrentThread.IsThreadPoolThread}");
            }, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
            continuation.GetAwaiter().OnCompleted((() =>
            {
                Console.WriteLine($"Continuation Task completed! Thread id {Thread.CurrentThread.ManagedThreadId}.  " +
                                  $"Is thread poll thread: {Thread.CurrentThread.IsThreadPoolThread}");
            }));
            secondTask.Start();
            Thread.Sleep(TimeSpan.FromSeconds(2));

            var thirdTask = new Task<int>(() =>
            {
                var innerTask = Task.Factory.StartNew(() => TaskLogging("third task inner", 5),
                    TaskCreationOptions.AttachedToParent);
                innerTask.ContinueWith(t => TaskLogging("third task inner continue", 2),
                    TaskContinuationOptions.AttachedToParent);
                return TaskLogging("task 3", 2);
            });
            thirdTask.Start();
            while (!thirdTask.IsCompleted)
            {
                Console.WriteLine(thirdTask.Status);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine(thirdTask.Status);

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        private static void TaskResultBlock()
        {
            var task1 = new Task<int>(() => TaskLogging("task 1", 2));
            task1.Start();
            var task1Result = task1.Result; // same as task.wait(): will block
            Console.WriteLine($"Result1 is: {task1Result}");

            var task2 = new Task<int>(() => TaskLogging("task 2", 1));
//            task2.RunSynchronously();
            task2.Start();
            var task2Result = task2.Result;
            Console.WriteLine($"Result2 is: {task2Result}");

            var task3 = new Task<int>(() => TaskLogging("task 3", 2));
            Console.WriteLine($"Status3 is: {task3.Status}");
            task3.Start();

            while (!task3.IsCompleted)
            {
                Console.WriteLine($"Status3 is: {task3.Status}");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            Console.WriteLine($"Status3 is: {task3.Status}");
            var task3Result = task3.Result;
            Console.WriteLine($"Result3 is: {task3Result}");
        }

        private static void StartTaskWay()
        {
            var task1 = new Task(() => TaskLogging("task 1", 2));
            var task2 = new Task(() => TaskLogging("task 2", 2));

            task2.Start();
            task1.Start();

            Task.Run(() => TaskLogging("Task 3", 2));

            Task.Factory.StartNew(() => TaskLogging("Task 4", 2));
            Task.Factory.StartNew(() => TaskLogging("Task 5", 2), TaskCreationOptions.LongRunning);

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        private static int TaskLogging(string name, int sleepTime)
        {
            Console.WriteLine($"Task {name} is running on a thread id {Thread.CurrentThread.ManagedThreadId}");
            Console.WriteLine($"Is thread poll thread: {Thread.CurrentThread.IsThreadPoolThread}");
            Thread.Sleep(TimeSpan.FromSeconds(sleepTime));
            return 16 * sleepTime;
        }

        private static void BadFaultyThread()
        {
            try
            {
                var badFaultyThread = new Thread(
                    () =>
                    {
                        Console.WriteLine("Staring a bad faulty thread....");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        throw new Exception("expection bad faulty");
                    });
                badFaultyThread.Start();
                badFaultyThread.Join();
            }
            catch (Exception)
            {
                Console.WriteLine("should not see me.");
            }
        }

        private static void FaultyThread()
        {
            var faultyThread = new Thread(
                () =>
                {
                    try
                    {
                        Console.WriteLine("Staring a faulty thread....");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        throw new Exception("expection faulty");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Expection handled: {e.Message}");
                    }
                });

            faultyThread.Start();
            faultyThread.Join();
        }

        private static void Closures()
        {
            var i = 10;
            var thread = new Thread(() => { Console.WriteLine(i); });

            thread.Start();

            i = 20;
            var thread1 = new Thread(() => { Console.WriteLine(i); });

            thread1.Start();

            thread.Join();
            thread1.Join();
        }

        private static void SetProcessorAffinity()
        {
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
        }

        private static void ThreadState()
        {
            var stopThread = new Thread(
                () => { Thread.Sleep(TimeSpan.FromSeconds(2)); });

            var thread = new Thread(
                () =>
                {
                    Console.WriteLine("ThreadState");
                    Console.WriteLine(Thread.CurrentThread.ThreadState.ToString());
                    for (var i = 0; i < 10; i++)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                        Console.WriteLine(i);
                    }
                });

            thread.Start();
            stopThread.Start();

            for (var i = 0; i < 30; i++)
            {
                Console.WriteLine(stopThread.ThreadState.ToString());
            }

            Thread.Sleep(TimeSpan.FromSeconds(6));

            stopThread.Abort(); // warning: should use cancellationToken

            Console.WriteLine("a thread has been aborted");
            Console.WriteLine(stopThread.ThreadState.ToString());
            Console.WriteLine(thread.ThreadState.ToString());
        }

        private static void TaskAndThread()
        {
            var task = new Task(
                () =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(16));
                    Console.WriteLine("Hello Task");
                });


            var thread = new Thread(
                () =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        Console.WriteLine(i);
                    }
                });

            task.Start();
            thread.Start();

            thread.Join();
//            Task.WaitAll(task);
        }
    }
}