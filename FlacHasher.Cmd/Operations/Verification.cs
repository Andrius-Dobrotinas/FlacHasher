﻿using Andy.FlacHash.Audio;
using Andy.FlacHash.Crypto;
using Andy.FlacHash.Hashfile.Read;
using Andy.FlacHash.Hashing;
using Andy.FlacHash.Verification;
using Andy.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash.Application.Cmd
{
    public static class Verification
    {
        public interface IHashfileParams
        {
            string HashFile { get; set; }
            string[] HashfileExtensions { get; set; }
            string HashfileEntrySeparator { get; set; }
            string InputDirectory { get; set; }
        }

        /// <summary>
        /// This currently assumes the hash file is in the same directory as the target files
        /// and just take the first hash file found
        /// </summary>
        public static void Verify(IList<FileInfo> targetFiles, IHashfileParams parameters, IAudioFileDecoder audioFileDecoder, Algorithm hashAlgorithm, IFileSearch fileSearch, CancellationToken cancellation)
        {
            var hashfile = GetHashFile(parameters, fileSearch);
            Console.Error.WriteLine($"Hashfile: {hashfile?.FullName}");
            if (hashfile == null || !hashfile.Exists)
                throw new InputFileMissingException("Hash file not found");

            var hashfileReader = HashFileReader.Default.BuildHashfileReader(parameters.HashfileEntrySeparator);
            var fileHashMap = hashfileReader.Read(hashfile);

            var hasher = BuildHasher(audioFileDecoder, hashAlgorithm);
            var verifier = BuildVerifier();

            Verify(hasher, verifier, targetFiles, fileHashMap, cancellation);
        }

        public static void Verify(IMultiFileHasher hasher, HashVerifier hashVerifier, IList<FileInfo> files, FileHashMap fileHashMap, CancellationToken cancellation)
        {
            var results = VerifyHashes(files, fileHashMap, hashVerifier, hasher, cancellation);

            WriteStdErrLine();
            WriteStdErrLine();
            WriteStdErrLine("======== Results =========");
            WriteStdErrLine("File\t=>\tIsMatch");
            foreach (var result in results)
            {
                WriteStdErrLine($"{result.Key.Name} => {result.Value}");
            }

            WriteStdErrLine("======== The End =========");
        }

        private static IList<KeyValuePair<FileInfo, HashMatch>> VerifyHashes(IList<FileInfo> files, FileHashMap expectedHashes, HashVerifier hashVerifier, IMultiFileHasher hasher, CancellationToken cancellation)
        {
            var filesUnique = files
                .Distinct(new FileInfoEqualityComprarer())
                .ToList();

            var fileHashes = HashEntryMatching.MatchFilesToHashes(expectedHashes, filesUnique);
            var results = new List<KeyValuePair<FileInfo, HashMatch>>(fileHashes.Count);

            foreach (FileHashResult calcResult in hasher.ComputeHashes(fileHashes.Keys, cancellation))
            {
                cancellation.ThrowIfCancellationRequested();

                if (calcResult.Exception == null)
                {
                    var isMatch = hashVerifier.DoesMatch(fileHashes, calcResult.File, calcResult.Hash);
                    Console.WriteLine($"{calcResult.File.Name} => {isMatch}");
                    results.Add(new KeyValuePair<FileInfo, HashMatch>(calcResult.File, isMatch ? HashMatch.True : HashMatch.False));
                }
                else
                {
                    var result = (calcResult.Exception is InputFileNotFoundException)
                            ? HashMatch.NotFound
                            : HashMatch.Error;
                    results.Add(new KeyValuePair<FileInfo, HashMatch>(calcResult.File, result));
                    Console.WriteLine($"{calcResult.File.Name} => Error: {calcResult.Exception.Message}");
                }

                //so it doesn't go back to the enumerator, which would result in launching decoding the next file
                cancellation.ThrowIfCancellationRequested();
            }

            return results;
        }

        public static HashVerifier BuildVerifier()
        {
            var hashFormatter = new PlainLowercaseHashFormatter();
            return new HashVerifier(hashFormatter);
        }

        public static IMultiFileHasher BuildHasher(IAudioFileDecoder reader, Algorithm hashAlgorithm)
        {
            var hasher = new FileHasher(reader, new Hasher(hashAlgorithm));
            return new MultiFileHasher(hasher, continueOnError: true);
        }

        public static FileInfo GetHashFile(IHashfileParams parameters, IFileSearch fileSearch)
        {
            if (parameters.HashFile != null)
            {
                var isAbsolute = Path.IsPathRooted(parameters.HashFile);
                if (!isAbsolute && parameters.InputDirectory != null)
                    return new FileInfo(Path.Combine(parameters.InputDirectory, parameters.HashFile));
                else
                    return new FileInfo(parameters.HashFile);
            }
            else if (parameters.InputDirectory != null)
            {
                var hashfileExtensions = FileExtension.PrefixWithDot(parameters.HashfileExtensions);
                WriteStdErrLine($"Looking for a hashfile with extension(s): {string.Join(',', hashfileExtensions)}");

                return fileSearch.FindFiles(new DirectoryInfo(parameters.InputDirectory), "*")
                    .FirstOrDefault(file => hashfileExtensions.Contains(file.Extension));
            }
            else
                return null;
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