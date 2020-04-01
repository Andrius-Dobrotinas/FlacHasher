using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public class ParameterReader
    {
        /// <summary>
        /// Extracts parameters from the dictionary and validates them making sure the correct combination of said parameters is provided
        /// </summary>
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey(ArgumentNames.Decoder))
            {
                throw new CmdLineArgNotFoundException($"The decoder executable file has not been specified. Use {ArgumentNames.Decoder}= option");
            }

            var decoderPath = arguments[ArgumentNames.Decoder];
            var decoder = new FileInfo(decoderPath);

            if (!arguments.ContainsKey(ArgumentNames.Input))
            {
                throw new CmdLineArgNotFoundException($"The input file has not been specified. Use {ArgumentNames.Input}= option");
            }

            var inputPathString = arguments[ArgumentNames.Input];
            var paths = inputPathString.Split(';');
            var filesAndDirs = GetInputFilesAndDirs(paths);

            if (filesAndDirs.Item1.Count == 0 && filesAndDirs.Item2.Count == 0)
            {
                throw new CmdLineArgNotFoundException("At least one input file must be specified");
            }

            var args = new Parameters
            {
                Decoder = decoder,
                InputFiles = filesAndDirs.Item1,
                InputDirectories = filesAndDirs.Item2
            };

            if (args.InputDirectories.Any())
            {
                if (!arguments.ContainsKey(ArgumentNames.FileExtension) || string.IsNullOrEmpty(arguments[ArgumentNames.Input]))
                {
                    throw new CmdLineArgNotFoundException($"At least one directory was specified as input, but not no file extension has been specified. Use {ArgumentNames.FileExtension}= option");
                }

                args.TargetFileExtension = arguments[ArgumentNames.FileExtension];
            }

            if (arguments.ContainsKey(ArgumentNames.OutputFormat))
            {
                args.OutputFormat = arguments[ArgumentNames.OutputFormat];
            }

            return args;
        }

        private static Tuple<IReadOnlyCollection<FileInfo>, IReadOnlyCollection<DirectoryInfo>> GetInputFilesAndDirs(IEnumerable<string> paths)
        {
            var dirPaths = paths.Where(IsDirectory).ToArray();
            var filePaths = paths.Except(dirPaths).ToArray();

            var files = filePaths
                .Select(path => new FileInfo(path))
                .ToArray();

            var dirs = dirPaths
                .Select(path => new DirectoryInfo(path))
                .ToArray();

            return new Tuple<IReadOnlyCollection<FileInfo>, IReadOnlyCollection<DirectoryInfo>>(files, dirs);
        }

        private static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}