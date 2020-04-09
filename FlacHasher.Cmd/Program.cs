using Andy.FlacHash.Cmd.CommandLine;
using Andy.FlacHash.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    class Program
    {
        const string settingsFileName = "settings.cfg";

        static int Main(string[] args)
        {
            Settings settings;
            try
            {
                var settingsFile = new FileInfo(settingsFileName);
                settings = SettingsProvider.GetSettings(settingsFile);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed reading a settings file. {e.Message}");
                return (int)ReturnValue.SettingsReadingFailure;
            }

            Parameters parameters;
            try
            {
                var argumentDictionary = ParseArguments(args);
                parameters = ParameterReader.GetParameters(argumentDictionary);
            }
            catch (CmdLineArgNotFoundException e)
            {
                Console.Error.WriteLine(e.Message);
                return (int)ReturnValue.ArgumentNotFound;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return (int)ReturnValue.ArgumentError;
            }

            try
            {
                var decoderFile = GetADecoder(settings, parameters);
                var decoder = new Input.Flac.CmdLineDecoder(decoderFile);
                var hasher = new FileHasher(decoder, new Sha256HashComputer());
                var multiHasher = new MultipleFileHasher(hasher);
                var directoryHasher = new DirectoryHasher(multiHasher);

                IList<FileHashResult> hashes;

                if (parameters.InputFiles.Any())
                {
                    hashes = multiHasher
                        .ComputeHashes(parameters.InputFiles)
                        .ToArray();
                }
                else
                {
                    hashes = new FileHashResult[0];
                }

                if (parameters.InputDirectories.Any())
                {
                    var hashes2 = DoADirectory(directoryHasher, parameters.InputDirectories, parameters.TargetFileExtension);
                    hashes = hashes.Concat(hashes2).ToArray();
                }

                // TODO: maybe it's better to output each hash as it's computed?
                foreach (var entry in hashes)
                {
                    OutputHash(entry.Hash, parameters.OutputFormat, entry.File);
                };
            }
            catch (ConfigurationException e)
            {
                Console.WriteLine(e.Message);

                return (int)ReturnValue.ConfigurationError;
            }
            catch (Input.InputReadingException e)
            {
                Console.WriteLine(e.Message);

                return (int)ReturnValue.InputReadingFailure;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return (int)ReturnValue.ExecutionFailure;
            }

            Console.Error.WriteLine("Done!");

            return (int)ReturnValue.Success;
        }

        private static IList<FileHashResult> DoADirectory(DirectoryHasher directoryHasher, IEnumerable<DirectoryInfo> inputDirectories, string fileExtension)
        {
            var fileSearchPattern = $"*.{fileExtension}";

            return inputDirectories
                .SelectMany(
                    directory => directoryHasher.ComputeHashes(directory, fileSearchPattern))
                .ToArray(); // TODO save dir info and group results by dirs for outputting
        }

        private static void OutputHash(byte[] hash, string format, FileInfo sourceFile)
        {
            if (string.IsNullOrEmpty(format))
            {
                Console.OpenStandardOutput().Write(hash, 0, hash.Length);
                Console.Error.WriteLine();
            }
            else
            {
                string formattedOutput = OutputFormatter.GetFormattedString(format, hash, sourceFile);
                Console.WriteLine(formattedOutput);
            }
        }

        private static FileInfo GetADecoder(Settings settings, Parameters cmdlineArguments)
        {
            return cmdlineArguments.Decoder ?? settings.Decoder ?? throw new ConfigurationException($"A Decoder has not been specified. Either specify it the settings file or provide it as a parameter {ArgumentNames.Decoder} to the command");
        }

        private static IDictionary<string, string> ParseArguments(string[] args)
        {
            var argParser = new ArgumentParser('=');

            return args.Select(argParser.ParseArgument)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value);
        }
    }
}