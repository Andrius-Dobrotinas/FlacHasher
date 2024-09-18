using Andy.FlacHash.Hashing;
using Andy.FlacHash.Hashing.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class ExecutionParameterResolver
    {
        public static IList<FileInfo> GetInputFiles(ApplicationSettings settings, FileSearch fileSearch)
        {
            if (settings.InputFiles != null)
            {
                return settings.InputFiles
                    .Select(path => new FileInfo(path))
                    .ToArray();
            }
            if (settings.InputDirectory != null)
            {
                var fileExtension = settings.TargetFileExtension;

                // TODO: define default extension in code, somewhere with a decoder?..
                if (fileExtension == null)
                    throw new ConfigurationException("Target file extension must be specified when scanning a directory");

                return fileSearch.FindFiles(new DirectoryInfo(settings.InputDirectory), fileExtension)
                    .ToList();
            }

            throw new ConfigurationException("No input files or directory has been specified");
        }

        public static Algorithm ParseHashAlgorithmOrGetDefault(string algo)
        {
            if (algo == null)
                return Settings.Defaults.HashAlgorithm;

            Algorithm value;
            if (!Enum.TryParse(algo.ToUpperInvariant(), out value))
                throw new ConfigurationException($"The specified Hash algorithm is not supported: {algo}");
            return value;
        }
    }
}