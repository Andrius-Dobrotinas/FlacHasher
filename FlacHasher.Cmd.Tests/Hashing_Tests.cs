using Andy.FlacHash.Audio;
using Andy.FlacHash.Crypto;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Application.Cmd
{
    public class Hashing_Tests
    {
        [Test]
        public void Hashing_multiple_files__When_one_fails_to_decode__Must_throw_and_never_reach_the_files_after_it()
        {
            var goodFile = new FileInfo("good.flac");
            var failingFile = new FileInfo("bad.flac");
            var nextFile = new FileInfo("after.flac");

            var decoder = new Mock<IAudioFileDecoder>();
            decoder.Setup(d => d.Read(goodFile, It.IsAny<CancellationToken>()))
                .Returns(() => new DecoderStream(new MemoryStream(new byte[] { 1, 2, 3 })));
            decoder.Setup(d => d.Read(failingFile, It.IsAny<CancellationToken>()))
                .Throws(new GenericDecoderException(new Exception("simulated decode failure")));
            decoder.Setup(d => d.Read(nextFile, It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("must never be reached"));

            // ComputeHashes writes to Console on success; keep that out of the test's own output
            var originalOut = Console.Out;
            Console.SetOut(TextWriter.Null);
            try
            {
                // A real continueOnError:true would swallow the failure and go on to nextFile instead of throwing here
                Assert.Throws<GenericDecoderException>(() =>
                    Hashing.ComputeHashes(
                        new[] { goodFile, failingFile, nextFile },
                        outputFomat: "{hash}",
                        decoder.Object,
                        printProcessProgress: false,
                        Algorithm.MD5,
                        CancellationToken.None));
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
