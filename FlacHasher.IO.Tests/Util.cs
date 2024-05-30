using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FlacHash.IO
{
    static class Util
    {
        public static byte[] Read(Stream stream)
        {
            using (var testStream = new MemoryStream())
            {
                stream.CopyTo(testStream);
                return testStream.ToArray();
            }
        }

        public static byte[] Read(Stream stream, CancellationToken cancellation)
        {
            using (var testStream = new MemoryStream())
            {
                stream.CopyToAsync(testStream, cancellation).GetAwaiter().GetResult();
                return testStream.ToArray();
            }
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
