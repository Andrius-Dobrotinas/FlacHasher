using Andy.Cmd;
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
                var argumentDictionary = ArgumentSplitter.GetArguments(args);
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
                FileInfo decoderFile = ExecutionParameterResolver.GetDecoder(settings, parameters);
                string outputFomat = ExecutionParameterResolver.ResolveOutputFormat(settings, parameters);
                IList<FileInfo> inputFiles = ExecutionParameterResolver.GetInputFiles(parameters);

                var decoder = new IO.Audio.Flac.CmdLineFileDecoder(
                    decoderFile,
                    new ExternalProcess.ProcessRunner());

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
            catch (IO.InputReadingException e)
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
    }
}