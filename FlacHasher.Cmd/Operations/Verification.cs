using Andy.Cmd.Parameter;
using Andy.FlacHash.Audio;
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
        public interface IVerificationParams
        {
            bool InputIgnoreExtraneous { get; set; }
        }

        /// <summary>
        /// This currently assumes the hash file is in the same directory as the target files
        /// and just take the first hash file found
        /// </summary>
        public static void Verify(VerificationParameters parameters, IAudioFileDecoder audioFileDecoder, IFileSearch fileSearch, CancellationToken cancellation)
        {
            var hashfile = GetHashFile(parameters, fileSearch);
            Console.Error.WriteLine($"Hashfile: {hashfile?.FullName}");
            if (hashfile == null || !hashfile.Exists)
                throw new InputFileMissingException("Hash file not found");

            var hashfileReader = HashFileReader.Default.BuildHashfileReader(parameters.HashfileEntrySeparator);
            var fileHashMap = hashfileReader.Read(hashfile);

            var targetFiles = FindFiles(hashfile, fileHashMap, parameters, fileSearch).ToArray();

            var hasher = BuildHasher(audioFileDecoder, parameters.HashAlgorithm);
            var verifier = new HashVerifier();

            Verify(hasher, verifier, targetFiles, fileHashMap, parameters, cancellation);
        }

        public static void Verify(IMultiFileHasher hasher, HashVerifier hashVerifier, IList<FileInfo> files, FileHashMap fileHashMap, IVerificationParams parameters, CancellationToken cancellation)
        {
            var results = VerifyHashes(files, fileHashMap, hashVerifier, hasher, parameters, cancellation);

            WriteStdErrLine();
            WriteStdErrLine();
            WriteStdErrLine("RESULTS".PadLeft(25));
            WriteStdErrLine();
            WriteStdErrLine($"{"==================== File ".PadRight(50, '=')}|= Hash Match ==");
            foreach (var result in results)
            {
                WriteStdErrLine($"{result.Key.Name.PadRight(50, '.')}|  {HashMatchValueFormatter.GetString(result.Value)}");
            }
        }

        private static IList<KeyValuePair<FileInfo, HashMatch>> VerifyHashes(IList<FileInfo> files, FileHashMap expectedHashes, HashVerifier hashVerifier, IMultiFileHasher hasher, IVerificationParams parameters, CancellationToken cancellation)
        {
            var filesUnique = files
                .Distinct(new FileInfoEqualityComprarer())
                .ToList();

            var fileHashes = HashEntryMatching.MatchFilesToHashes(expectedHashes, filesUnique);
            var extraneousFiles = fileHashes.Where(x => x.Value == null).ToList();
            var expectedFiles = fileHashes.Except(extraneousFiles).Select(x => x.Key);

            var results = new List<KeyValuePair<FileInfo, HashMatch>>(fileHashes.Count);

            foreach (FileHashResult hashingResult in hasher.ComputeHashes(expectedFiles, cancellation))
            {
                cancellation.ThrowIfCancellationRequested();

                if (hashingResult.Exception == null)
                {
                    var isMatch = hashVerifier.DoesMatch(fileHashes, hashingResult.File, hashingResult.Hash);
                    var result = isMatch ? HashMatch.Match : HashMatch.NoMatch;

                    Console.WriteLine($"{hashingResult.File.Name} => {HashMatchValueFormatter.GetString(result)}");
                    results.Add(new KeyValuePair<FileInfo, HashMatch>(hashingResult.File, result));
                }
                else
                {
                    var result = (hashingResult.Exception is InputFileNotFoundException)
                            ? HashMatch.NotFound
                            : HashMatch.Error;
                    results.Add(new KeyValuePair<FileInfo, HashMatch>(hashingResult.File, result));
                    Console.WriteLine($"{hashingResult.File.Name} => Error: {hashingResult.Exception.Message}");
                }

                //so it doesn't go back to the enumerator, which would result in launching decoding the next file
                cancellation.ThrowIfCancellationRequested();
            }

            if (!parameters.InputIgnoreExtraneous)
                foreach (var file in extraneousFiles.Select(x => x.Key))
                    results.Add(new KeyValuePair<FileInfo, HashMatch>(file, HashMatch.NotExpected));

            return results;
        }

        public static HashVerifier BuildVerifier()
        {
            var hashFormatter = new PlainLowercaseHashFormatter();
            return new HashVerifier();
        }

        public static IMultiFileHasher BuildHasher(IAudioFileDecoder reader, Algorithm hashAlgorithm)
        {
            var hasher = new FileHasher(reader, new Hasher(hashAlgorithm));
            return new MultiFileHasher(hasher, continueOnError: true);
        }

        public static FileInfo GetHashFile(VerificationParameters parameters, IFileSearch fileSearch)
        {
            // hashfile && input dir && input files == no
            if (parameters.HashFile != null)
            {
                var isAbsolute = Path.IsPathRooted(parameters.HashFile);
                if (!isAbsolute && parameters.InputDirectory != null)
                    return new FileInfo(Path.Combine(parameters.InputDirectory, parameters.HashFile));
                else
                    return new FileInfo(parameters.HashFile);
            }
            // no hashfile specced
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

        /* infer target files here:
        * 1. hashfile only -- take files DEFINED in the hashfile, from hashfile directory (if relative, then take files from "current")
        * 2. hashfile and input dir -- take files DEFINED in the hashfile, from the specced dir
        * 3. hashfile && input files -ok use case, MOSTLY for position-based.
        * 4. input dir only
        * * should position-based hashes specificly demand a list of files, no input dirs? nah
        */
        public static IEnumerable<FileInfo> FindFiles(FileInfo resolvedHashfile, FileHashMap fileHashMap, VerificationParameters parameters, IFileSearch fileSearch)
        {
            if (parameters.HashFile != null)
            {
                if (parameters.InputDirectory != null)
                    return fileSearch.FindFiles(new DirectoryInfo(parameters.InputDirectory), parameters.TargetFileExtensions);
                else if (parameters.InputFiles != null)
                    return parameters.InputFiles.Select(x => new FileInfo(x));
                else // hashfile only
                {
                    var baseDirPath = resolvedHashfile.Directory.FullName;

                    if (fileHashMap.IsPositionBased)
                    {
                        if (parameters.TargetFileExtensions == null || !parameters.TargetFileExtensions.Any())
                            throw new ParameterMissingException($"Position-based hashfile requires target file extension", typeof(VerificationParameters).GetProperty(nameof(VerificationParameters.TargetFileExtensions)));

                        return fileSearch.FindFiles(new DirectoryInfo(baseDirPath), parameters.TargetFileExtensions).ToArray();
                    }
                    else
                    {
                        return fileHashMap.Hashes
                            .Select(
                                x => new FileInfo(Path.Combine(baseDirPath, x.Key)))
                            .ToArray();
                    }
                }
            }
            // hashfile was null
            else if (parameters.InputDirectory != null)
                return fileSearch.FindFiles(new DirectoryInfo(parameters.InputDirectory), parameters.TargetFileExtensions);
            else
                throw new ConfigurationException("Bad input files parameter combination");
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