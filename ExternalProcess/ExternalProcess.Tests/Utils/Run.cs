using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andy
{
    public static class Run
    {
        public static Task WaitWithTimeout(Action action, int timeoutMs)
        {
            // Use LongRunning to ensure the monitored action can start immediately even if the thread pool is saturated with other long-lived tasks.
            var task = Task.Factory.StartNew(
                action,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, //DenyChildAttach is used when Task.Run - keeping that logic here
                TaskScheduler.Default);

            return WaitWithTimeout(task, timeoutMs);

        }

        public static TTask WaitWithTimeout<TTask>(TTask task, int timeoutMs)
            where TTask : Task
        {
            using (var testTimeout = new CancellationTokenSource())
            {
                try
                {
                    testTimeout.CancelAfter(timeoutMs);

                    Task.WaitAll(new[] { task }, testTimeout.Token);
                    return task;
                }
                catch (OperationCanceledException)
                {
                    throw new TestTimeoutException();
                }
            }
        }

        public static T WithAutoCancellation<T>(Func<CancellationToken, T> func, int timeoutMs)
        {
            using (var testTimeout = new CancellationTokenSource())
            {
                testTimeout.CancelAfter(timeoutMs);
                return func(testTimeout.Token);
            }
        }
    }
}
