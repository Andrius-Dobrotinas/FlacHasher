using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public static class ExecutionParameterResolver
    {
        /// <summary>
        /// Returns user-provider value if one was provided. Otherwise returns one from the settings file.
        /// Empty strings are treated as valid values that indicate not to use formatting (ie equal Null).
        /// </summary>
        public static string ResolveOutputFormat(Settings settings, Parameters cmdlineArguments)
        {
            if (cmdlineArguments.OutputFormat != null)
            {
                if (cmdlineArguments.OutputFormat == "")
                    return null;
                return cmdlineArguments.OutputFormat;
            }

            if (settings.OutputFormat != null)
            {
                if (settings.OutputFormat == "")
                    return null;
                return settings.OutputFormat;
            }

            return null;
        }

        public static IList<FileInfo> GetInputFiles(Parameters cmdlineArguments, Settings settings)
        {
            if (cmdlineArguments.InputFiles != null)
            {
                return cmdlineArguments.InputFiles
                    .Select(path => new FileInfo(path))
                    .ToArray();
            }
            if (cmdlineArguments.InputDirectory != null)
            {
                var fileExtension = cmdlineArguments.TargetFileExtension;

                // TODO: define default extension in code, somewhere with a decoder?..
                if (String.IsNullOrEmpty(fileExtension))
                    throw new Exception("Target file exception must be specified when scanning a directory");

                DirectoryScanner.GetFiles(new DirectoryInfo(fileExtension), fileExtension);
            }

            throw new Exception("No input files or directory have been specified");
        }

        public static FileInfo GetDecoder(Settings settings, Parameters cmdlineArguments)
        {
            if (cmdlineArguments.Decoder != null)
                return new FileInfo(cmdlineArguments.Decoder);

            return settings.Decoder ?? throw new ConfigurationException($"A Decoder has not been specified. Either specify it the settings file or provide it as a parameter {ArgumentNames.Decoder} to the command");
        }
    }
}