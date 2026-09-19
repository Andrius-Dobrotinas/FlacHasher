using System.IO;
using System.Threading;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Says when it's been disposed of, so a test can tell whether the runner takes ownership of the stream it's given.
    /// </summary>
    class DisposeSignallingMemoryStream : MemoryStream
    {
        readonly ManualResetEventSlim disposeSignal;

        public DisposeSignallingMemoryStream(byte[] source, ManualResetEventSlim disposeSignal) : base(source)
        {
            this.disposeSignal = disposeSignal;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            disposeSignal.Set();
        }
    }
}
