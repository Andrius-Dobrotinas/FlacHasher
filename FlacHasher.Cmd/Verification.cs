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

        public static void Verify(IMultipleFileHasher hasher, HashVerifier hashVerifier, IList<FileInfo> files, FileHashMap fileHashMap, CancellationToken cancellation)
        {
            var (results, resultsMissing) = VerifyHashes(files, fileHashMap, hashVerifier, hasher, cancellation);
          
            WriteStdErrLine();
            WriteStdErrLine();
            WriteStdErrLine("======== Results =========");
            WriteStdErrLine("File\t=>\tIsMatch");
            foreach (var result in results)
            {
                WriteStdErrLine($"{result.Key} => {result.Value}");
            }

            foreach (var result in resultsMissing)
            {
                WriteStdErrLine($"{result.Name} Not Found");
            }
            WriteStdErrLine("======== The End =========");
        }

        private static (IList<KeyValuePair<FileInfo, bool>> results, IList<FileInfo> missingFiles) 
            VerifyHashes(IList<FileInfo> files, FileHashMap expectedHashes, HashVerifier hashVerifier, IMultipleFileHasher hasher, CancellationToken cancellation)
        {
            var (existingFileHashes, missingFileHashes) = HashEntryMatching.MatchFilesToHashes(expectedHashes, files);
            var existingFileHashDictionary = existingFileHashes.ToDictionary(x => x.Key, x => x.Value);
            
            var results = new List<KeyValuePair<FileInfo, bool>>();

            foreach (FileHashResult calcResult in hasher.ComputeHashes(existingFileHashDictionary.Keys, cancellation))
                {
                    cancellation.ThrowIfCancellationRequested();

                if (calcResult.Exception == null)
                {
                    var isMatch = hashVerifier.DoesMatch(existingFileHashDictionary, calcResult.File, calcResult.Hash);
                    Console.WriteLine($"{calcResult.File.Name} => {isMatch}");
                    results.Add(new KeyValuePair<FileInfo, bool>(calcResult.File, isMatch));
                }
                else
                {
                    Console.WriteLine($"{calcResult.File.Name} => Error: {calcResult.Exception.Message}");
                }

                //so it doesn't go back to the enumerator, which would result in launching decoding the next file
                cancellation.ThrowIfCancellationRequested();
            }

            return (results, missingFileHashes.Select(x => x.Key).ToList());
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

        public static IMultipleFileHasher BuildHasher(FileInfo decoderFile, ProcessRunner processRunner, bool continueOnError)
        {
            var steamFactory = new IO.ReadStreamFactory();
            var decoder = new IO.Audio.Flac.CmdLine.StreamDecoder(
                decoderFile,
                processRunner);
            var reader = new IO.Audio.FileStreamDecoder(steamFactory, decoder);

            var hasher = new FileHasher(reader, new Crypto.Sha256HashComputer());
            return new MultipleFileHasher(hasher, continueOnError);
        }

        static void WriteStdErrLine(string text)
        {
            Console.Error.WriteLine(text);
        }

        static void WriteStdErrLine()
        {
            Console.Error.WriteLine();
        }
    }
}