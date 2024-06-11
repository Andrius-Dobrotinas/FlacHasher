using Andy.FlacHash.Crypto;
using Andy.FlacHash.ExternalProcess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Cmd
{
    static class Computation
    {
        const char newlineChar = '\n';

        public static void ComputeHashes(IList<FileInfo> inputFiles, string outputFomat, FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError, bool printProcessProgress, Action<string> writeUserLine, CancellationToken cancellation)
        {
            var hasher = BuildHasher(decoderFile, processRunner, continueOnError);
            ComputeHashes(hasher, inputFiles, outputFomat, printProcessProgress, writeUserLine, cancellation);
        }

        static MultipleFileHasher BuildHasher(FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError)
        {
            var decoder = new IO.Audio.Flac.CmdLine.FileDecoder(
                    decoderFile,
                    processRunner);

            var hasher = new FileHasher(decoder, new Sha256HashComputer());
            return new MultipleFileHasher(hasher, continueOnError);
        }

        static void ComputeHashes(MultipleFileHasher multiHasher, IEnumerable<FileInfo> inputFiles, string outputFormat, bool printProcessProgress, Action<string> writeUserLine, CancellationToken cancellation)
        {
            IEnumerable<FileHashResult> computations = multiHasher
                    .ComputeHashes(inputFiles, cancellation);

            // The hashes should be computed on this enumeration, and therefore will be output as they're computed
            foreach (var result in computations)
            {
                if (result.Exception == null)
                    OutputHash(result.Hash, outputFormat, result.File);
                else
                    if (!printProcessProgress) // Decoder's output would indicate an error
                        writeUserLine($"Error processing file {result.File.Name}: {result.Exception.Message}");
            };
        }

        static void OutputHash(byte[] hash, string format, FileInfo sourceFile)
        {
            if (string.IsNullOrEmpty(format))
            {
                var stdout = Console.OpenStandardOutput();
                stdout.Write(hash, 0, hash.Length);
                stdout.Write(stackalloc byte[] { (byte)newlineChar });
            }
            else
            {
                string formattedOutput = OutputFormatting.GetFormattedString(format, hash, sourceFile);
                Console.WriteLine(formattedOutput);
            }
        }
    }
}
