using Andy.FlacHash.ExternalProcess;
using Andy.FlacHash.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash.Cmd
{
    public static class Verification
    {
        /// <summary>
        /// This currently assumes the hash file is in the same directory as the target files
        /// and just take the first hash file found
        /// </summary>
        public static void Verify(IList<FileInfo> targetFiles, Parameters parameters, FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError, CancellationToken cancellation)
        {
            var hashfile = DirectoryScanner.GetFiles(new DirectoryInfo(parameters.InputDirectory), "hash").FirstOrDefault();
            if (hashfile == null)
                throw new TargetFileNotFoundException("Hash file not found");

            var hashfileReader = BuildHashfileReader();
            var fileHashMap = hashfileReader.Read(hashfile);

            var hasher = BuildHasher(decoderFile, processRunner, continueOnError);
            var verifier = BuildVerifier();

            Verify(hasher, verifier, targetFiles, fileHashMap, cancellation);
        }

        public static void Verify(IReportingMultipleFileHasher hasher, HashVerifier hashVerifier, IList<FileInfo> files, FileHashMap fileHashMap, CancellationToken cancellation)
        {
            var (results, resultsMissing) = VerifyHashes(files, fileHashMap, hashVerifier, hasher, cancellation);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("======== Results =========");
            Console.WriteLine("File\t=>\tIsMatch");
            foreach (var result in results)
            {
                ReportResult(result.Key, result.Value);
            }

            foreach (var result in resultsMissing)
            {
                Console.WriteLine($"{result.Name} Not Found");
            }
            Console.WriteLine("======== The End =========");
        }

        private static (IList<KeyValuePair<FileInfo, bool>> results, IList<FileInfo> missingFiles)
        VerifyHashes(IList<FileInfo> files, FileHashMap expectedHashes, HashVerifier hashVerifier, IReportingMultipleFileHasher hasher, CancellationToken cancellation)
        {
            var (existingFileHashes, missingFileHashes) = HashEntryMatching.MatchFilesToHashes(expectedHashes, files);
            var existingFileHashDictionary = existingFileHashes.ToDictionary(x => x.Key, x => x.Value);

            var results = new List<KeyValuePair<FileInfo, bool>>();

            hasher.ComputeHashes(existingFileHashDictionary.Keys,
                (FileHashResult calcResult) =>
                {
                    if (calcResult.Exception == null)
                    {
                        var isMatch = hashVerifier.DoesMatch(existingFileHashDictionary, calcResult.File, calcResult.Hash);
                        ReportResult(calcResult.File, isMatch);
                        results.Add(new KeyValuePair<FileInfo, bool>(calcResult.File, isMatch));
                    }
                    else
                    {
                        Console.WriteLine($"Error processing file {calcResult.File.Name}: {calcResult.Exception.Message}");
                    }
                },
                cancellation);

            return (results, missingFileHashes.Select(x => x.Key).ToList());
        }

        static void ReportResult(FileInfo file, bool isMatch)
        {
            Console.WriteLine($"{file.Name} => {isMatch}");
        }

        public static HashFileReader BuildHashfileReader()
        {
            return new HashFileReader(
                        new HashMapParser(
                            new HashEntryCollectionParser(
                                new HashEntryParser()),
                            new CaseInsensitiveOrdinalStringComparer()));
        }

        public static HashVerifier BuildVerifier()
        {
            var hashFormatter = new PlainLowercaseHashFormatter();
            return new HashVerifier(hashFormatter);
        }

        public static IReportingMultipleFileHasher BuildHasher(FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError)
        {
            var steamFactory = new IO.ReadStreamFactory();
            var decoder = new IO.Audio.Flac.CmdLine.StreamDecoder(
                decoderFile,
                processRunner);
            var reader = new IO.Audio.FileStreamDecoder(steamFactory, decoder);

            var hasher = new FileHasher(reader, new Crypto.Sha256HashComputer());
            var cancellableHasher = new ReportingMultipleFileHasher(
                new MultipleFileHasher(hasher, continueOnError));

            return cancellableHasher;
        }
    }
}