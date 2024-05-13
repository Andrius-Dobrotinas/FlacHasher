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
        const char newlineChar = '\n';
        const int processExitTimeoutMsDefault = 2000;
        const int processTimeoutSecDefault = int.MaxValue;
        const bool printProcessProgress = true;

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
                WriteUserLine($"Failure reading a settings file. {e.Message}");
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
                WriteUserLine(e.Message);
                return (int)ReturnValue.ArgumentNotFound;
            }
            catch (Exception e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.ArgumentError;
            }

            try
            {
                FileInfo decoderFile = ExecutionParameterResolver.GetDecoder(settings, parameters);
                string outputFomat = ExecutionParameterResolver.ResolveOutputFormat(settings, parameters);
                int processExitTimeoutMs = ExecutionParameterResolver.GetProcessExitTimeoutInMs(settings, parameters, processExitTimeoutMsDefault);
                int processTimeoutSec = ExecutionParameterResolver.GetProcessTimeoutInSeconds(settings, parameters, processTimeoutSecDefault);
                IList<FileInfo> inputFiles = ExecutionParameterResolver.GetInputFiles(parameters);

                if (!inputFiles.Any())
                {
                    WriteUserLine("No files provided/found");
                    return (int)ReturnValue.NoFilesToProcess;
                }

                var decoder = new IO.Audio.Flac.CmdLine.FileDecoder(
                    decoderFile,
                    new ExternalProcess.ProcessRunner(processTimeoutSec, processExitTimeoutMs, printProcessProgress));

                var hasher = new FileHasher(decoder, new Sha256HashComputer());
                var multiHasher = new MultipleFileHasher(hasher);

                IEnumerable<FileHashResult> hashes = multiHasher
                        .ComputeHashes(inputFiles);

                // The hashes should be computed on this enumeration, and therefore will be output as they're computed
                foreach (var entry in hashes)
                {
                    if (entry.Exception == null)
                        OutputHash(entry.Hash, outputFomat, entry.File);
                    else
                        if (!printProcessProgress)
                            WriteUserLine($"Error processing file {entry.File.Name}: {entry.Exception.Message}");
                };
            }
            catch (ConfigurationException e)
            {
                WriteUserLine(e.Message);

                return (int)ReturnValue.ConfigurationError;
            }
            catch (IO.IOException e)
            {
                WriteUserLine(e.Message);

                return (int)ReturnValue.InputReadingFailure;
            }
            catch (Exception e)
            {
                WriteUserLine(e.Message);

                return (int)ReturnValue.ExecutionFailure;
            }

            WriteUserLine("Done!");

            return (int)ReturnValue.Success;
        }

        static void OutputHash(byte[] hash, string format, FileInfo sourceFile)
        {
            if (string.IsNullOrEmpty(format))
            {
                var stdout = Console.OpenStandardOutput();
                stdout.Write(hash, 0, hash.Length);
                stdout.Write(stackalloc byte[] { (byte)newlineChar });
            }
            else
            {
                string formattedOutput = OutputFormatting.GetFormattedString(format, hash, sourceFile);
                Console.WriteLine(formattedOutput);
            }
        }

        static void WriteUserLine(string text)
        {
            Console.Error.WriteLine(text);
        }
    }
}