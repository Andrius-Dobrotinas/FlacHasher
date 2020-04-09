using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public class ParameterReader
    {
        /// <summary>
        /// Extracts parameters from the dictionary and validates the values making sure all mandatory parameters are provided, and their combinations are correct combinations (for parameters that are optional/mandatory based on the presence of other parameters.
        /// The presence of input files/dirs is checked as well
        /// </summary>
        /// <exception cref="CmdLineArgNotFoundException">When a mandatory parameter is not provided</exception>
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            var decoder = GetDecoder(arguments);

            var inputFilesAndDirs = GetInput(arguments);

            var args = new Parameters
            {
                Decoder = decoder,
                InputFiles = inputFilesAndDirs.Item1,
                InputDirectories = inputFilesAndDirs.Item2,
                TargetFileExtension = inputFilesAndDirs.Item3
            };

            if (arguments.ContainsKey(ArgumentNames.OutputFormat))
            {
                args.OutputFormat = arguments[ArgumentNames.OutputFormat];
            }

            return args;
        }

        private static FileInfo GetDecoder(IDictionary<string, string> arguments)
        {
            if (arguments.ContainsKey(ArgumentNames.Decoder))
            {
                var decoderPath = arguments[ArgumentNames.Decoder];
                return new FileInfo(decoderPath);
            }

            return null;
        }

        private static Tuple<IReadOnlyCollection<FileInfo>, IReadOnlyCollection<DirectoryInfo>, string> GetInput(IDictionary<string, string> arguments)
        {
            var inputFilesAndDirs = GetInputFilesAndDirs(arguments);

            string fileExtension;

            if (inputFilesAndDirs.Item2.Any())
            {
                if (!arguments.ContainsKey(ArgumentNames.FileExtension) || string.IsNullOrEmpty(arguments[ArgumentNames.Input]))
                {
                    throw new CmdLineArgNotFoundException($"At least one directory was specified as input, but no file extension has been specified. Use {ArgumentNames.FileExtension}= option");
                }

                fileExtension = arguments[ArgumentNames.FileExtension];
            }
            else
            {
                fileExtension = null;
            }

            return new Tuple<IReadOnlyCollection<FileInfo>, IReadOnlyCollection<DirectoryInfo>, string>(inputFilesAndDirs.Item1, inputFilesAndDirs.Item2, fileExtension);
        }

        private static Tuple<IReadOnlyCollection<FileInfo>, IReadOnlyCollection<DirectoryInfo>> GetInputFilesAndDirs(IDictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey(ArgumentNames.Input))
            {
                throw new CmdLineArgNotFoundException($"The input file/directory has not been specified. Use {ArgumentNames.Input}= option");
            }

            var inputPathString = arguments[ArgumentNames.Input];
            var paths = inputPathString.Split(';');
            var filesAndDirs = GetInputFilesAndDirs(paths);

            if (filesAndDirs.Item1.Count == 0 && filesAndDirs.Item2.Count == 0)
            {
                throw new CmdLineArgNotFoundException("At least one input file must be specified");
            }

            return filesAndDirs;
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

        /// <summary>
        /// Throws an exception if file/directory doesn't exist
        /// </summary>
        private static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}