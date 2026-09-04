using Andy.ExternalProcess;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Audio
{
    /// <summary>
    /// Read-only.
    /// On any error, throws <see cref="GenericDecoderException"/> or a more specific <see cref="DecoderException"/>.
    /// </summary>
    public class DecoderStream : Stream
    {
        private readonly Stream source;

        public DecoderStream(Stream source)
        {
            this.source = source;
        }

        public override bool CanRead => Try(() => source.CanRead);
        public override bool CanSeek => Try(() => source.CanSeek);
        public override bool CanWrite => false;
        public override long Length => Try(() => source.Length);
        public override long Position
        {
            get => Try(() => source.Position);
            set => Try(() => { source.Position = value; return true; });
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Try(() => source.Read(buffer, offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Try(() => source.Seek(offset, origin));
        }

        protected override void Dispose(bool disposing)
        {
            Try(() => { source.Dispose(); return true; });

            base.Dispose(disposing);
        }

        private static TResult Try<TResult>(Func<TResult> operation)
        {
            try
            {
                return operation();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ExecutionException e)
            {
                throw new DecoderException(e);
            }
            catch (Exception e)
            {
                throw new GenericDecoderException(e);
            }
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
    }
}