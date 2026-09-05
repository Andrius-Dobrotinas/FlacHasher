using System;
using System.IO;
using System.Text;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// Collects a process' error output, keeping only the most recent bytes once the cap is reached.
    /// Progress reports come first and the error that matters comes last, so the tail is the part worth keeping.
    /// The reading task writes to it while whoever is reporting a failure reads from it, so every operation takes the lock.
    /// </summary>
    class ErrorOutputBuffer
    {
        public const int Unbounded = -1;

        readonly object gate = new object();
        readonly byte[] ring;
        readonly MemoryStream everything;
        int writePosition;
        bool hasWrapped;

        public ErrorOutputBuffer(int maxBytes)
        {
            if (maxBytes == Unbounded)
                everything = new MemoryStream();
            else if (maxBytes > 0)
                ring = new byte[maxBytes];
            else
                throw new ArgumentOutOfRangeException(nameof(maxBytes), maxBytes, $"Has to be a positive number of bytes, or {Unbounded} for no limit");
        }

        public void Write(byte[] source, int count)
        {
            lock (gate)
            {
                if (everything != null)
                {
                    everything.Write(source, 0, count);
                    return;
                }

                // More than the ring can hold: only its last bytes could survive anyway
                int start = Math.Max(0, count - ring.Length);
                for (int i = start; i < count; i++)
                {
                    ring[writePosition] = source[i];
                    writePosition++;

                    if (writePosition == ring.Length)
                    {
                        writePosition = 0;
                        hasWrapped = true;
                    }
                }
            }
        }

        /// <summary>
        /// Whatever has been collected so far. Safe to call while the reading is still going on, which is what
        /// lets a report be made without waiting for a stream that may never come to an end.
        /// </summary>
        public string GetText()
        {
            lock (gate)
            {
                if (everything != null)
                    return Encoding.UTF8.GetString(everything.GetBuffer(), 0, (int)everything.Length);

                if (!hasWrapped)
                    return Encoding.UTF8.GetString(ring, 0, writePosition);

                // Oldest bytes first: the ring starts wherever the next write would have gone.
                // A multi-byte character split by the cut comes out as a replacement character, which is the price of a byte-sized limit.
                var ordered = new byte[ring.Length];
                int tailLength = ring.Length - writePosition;
                Array.Copy(ring, writePosition, ordered, 0, tailLength);
                Array.Copy(ring, 0, ordered, tailLength, writePosition);

                return Encoding.UTF8.GetString(ordered);
            }
        }
    }
}
