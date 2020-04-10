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
                Console.WriteLine($"Failure reading a settings file. {e.Message}");
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
                FileInfo decoderFile = GetDecoderOrThrow(settings, parameters);
                string outputFomat = ExecutionParameterResolver.ResolveOutputFormat(settings, parameters);
                IList<FileInfo> inputFiles = GetInputFiles(parameters, settings);

                var decoder = new Input.Flac.CmdLineDecoder(decoderFile);
                var hasher = new FileHasher(decoder, new Sha256HashComputer());
                var multiHasher = new MultipleFileHasher(hasher);

                IEnumerable<FileHashResult> hashes = multiHasher
                        .ComputeHashes(inputFiles);

                // The hashes should be computed on this enumeration, and therefore will be output as they're computed
                foreach (var entry in hashes)
                {
                    OutputHash(entry.Hash, outputFomat, entry.File);
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

        private static void OutputHash(byte[] hash, string format, FileInfo sourceFile)
        {
            if (string.IsNullOrEmpty(format))
            {
                Console.OpenStandardOutput().Write(hash, 0, hash.Length);
                Console.Error.WriteLine(); // This is so next message is printed on the next line. Check whether this is actually ok
            }
            else
            {
                string formattedOutput = OutputFormatter.GetFormattedString(format, hash, sourceFile);
                Console.WriteLine(formattedOutput);
            }
        }

        private static FileInfo GetDecoderOrThrow(Settings settings, Parameters cmdlineArguments)
        {
            if (cmdlineArguments.Decoder != null)
                return new FileInfo(cmdlineArguments.Decoder);

            return settings.Decoder ?? throw new ConfigurationException($"A Decoder has not been specified. Either specify it the settings file or provide it as a parameter {ArgumentNames.Decoder} to the command");
        }

        private static IList<FileInfo> GetInputFiles(Parameters cmdlineArguments, Settings settings)
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