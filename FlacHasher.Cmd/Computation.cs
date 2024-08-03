using Andy.ExternalProcess;
using Andy.FlacHash.Hashing;
using Andy.FlacHash.Hashing.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Andy.FlacHash.Cmd
{
    static class Computation
    {
        const char newlineChar = '\n';

        public static void ComputeHashes(IList<FileInfo> inputFiles, string outputFomat, FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError, bool printProcessProgress, Algorithm hashAlgorithm, CancellationToken cancellation)
        {
            var hasher = BuildHasher(decoderFile, processRunner, continueOnError, hashAlgorithm);
            ComputeHashes(hasher, inputFiles, outputFomat, printProcessProgress, cancellation);
        }

        static MultiFileHasher BuildHasher(FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError, Algorithm hashAlgorithm)
        {
            var decoder = new Audio.Flac.CmdLine.FileDecoder(
                    decoderFile,
                    processRunner);

            var hasher = new FileHasher(decoder, new HashComputer(hashAlgorithm));
            return new MultiFileHasher(hasher, continueOnError);
        }

        static void ComputeHashes(MultiFileHasher multiHasher, IEnumerable<FileInfo> inputFiles, string outputFormat, bool printProcessProgress, CancellationToken cancellation)
        {
            // On cancellation, simply errors out
            IEnumerable<FileHashResult> computations = multiHasher
                    .ComputeHashes(inputFiles, cancellation);

            var results = new List<FileHashResult>();
            // The hashes should be computed on this enumeration, and therefore will be output as they're computed
            foreach (var result in computations)
            {
                if (result.Exception == null)
                {
                    WriteHashToStdout(result.Hash, outputFormat, result.File);
                    results.Add(result);
                }
                else
                    if (!(result.Exception is ExecutionException) || printProcessProgress)
                        WriteStdErrLine($"Error processing file {result.File.Name}: {result.Exception.Message}");
            };

            /* Summary at the end just for user's convenience.
             * Results get written to stdout without any clutter, but for those who read console output,
             * this presents the results in one place without having to go through the whole process' progress output.*/
            if (printProcessProgress && !string.IsNullOrEmpty(outputFormat))
            {
                WriteStdErrLine("\n======== Results =========");
                foreach (var result in results)
                {
                    string formattedOutput = OutputFormatting.GetFormattedString(outputFormat, result.Hash, result.File);
                    WriteStdErrLine(formattedOutput);
                }
                WriteStdErrLine("======== The End =========");
            }
        }

        /// <summary>
        /// Write hash to stdout: either raw hash bytes or, if <paramref name="format"/> is specified,
        /// a formatted string.
        /// </summary>
        static void WriteHashToStdout(byte[] hash, string format, FileInfo sourceFile)
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

        static void WriteStdErrLine(string text)
        {
            Console.Error.WriteLine(text);
        }
    }
}
