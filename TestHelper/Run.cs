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
            return WaitWithTimeout(Task.Run(action), timeoutMs);
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

        public static T WithAutoCancellation<T> (Func<CancellationToken, T> func, int timeoutMs)
        {
            using (var testTimeout = new CancellationTokenSource())
            {
                testTimeout.CancelAfter(timeoutMs);
                return func(testTimeout.Token);
            }
        }
    }
}
