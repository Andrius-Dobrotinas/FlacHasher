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

        public override bool CanRead => source.CanRead;
        public override bool CanSeek => source.CanSeek;
        public override bool CanWrite => false;
        public override long Length => source.Length;
        public override long Position { get => source.Position; set => source.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return source.Read(buffer, offset, count);
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

        public override long Seek(long offset, SeekOrigin origin)
        {
            try
            {
                return source.Seek(offset, origin);
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

        protected override void Dispose(bool disposing)
        {
            try
            {
                source.Dispose();
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

            base.Dispose(disposing);
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