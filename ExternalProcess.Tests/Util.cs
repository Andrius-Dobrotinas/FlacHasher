using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.ExternalProcess
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
    }
}
