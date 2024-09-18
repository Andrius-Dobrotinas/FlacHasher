using Andy.Cmd;
using Andy.Cmd.Parameter;
using Andy.ExternalProcess;
using Andy.FlacHash.Hashing.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Andy.FlacHash.Cmd
{
    class Program
    {
        const string settingsFileName = "settings.cfg";
        const int processExitTimeoutMsDefault = 2000;
        const int processStartWaitMsDefault = 100;
        const int processTimeoutSecDefault = ProcessRunner.NoTimeoutValue;
        const bool printProcessProgress = true;
        const bool continueOnErrorDefault = true;

        static int Main(string[] args)
        {
            bool lowercaseParams = true;
            var argumentDictionary = ArgumentSplitter.GetArguments(args, paramNamesToLowercase: lowercaseParams);
            Parameters parameters;
            try
            {
                parameters = ParameterReader.GetParameters<Parameters>(argumentDictionary, inLowercase: lowercaseParams);
            }
            catch (ParameterMissingException e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.ArgumentNotProvided;
            }
            catch (ParameterException e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.ArgumentError;
            }

            bool isVerification = argumentDictionary.ContainsKey(ParameterNames.ModeVerify);
            ApplicationSettings settings;
            try
            {
                var settingsFile = new FileInfo(settingsFileName);

                var settingsFromFileDictionary = SettingsProvider.GetSettingsDictionary(settingsFile, parameters.Profile)
                    .ToDictionary(x => x.Key, x => new[] { x.Value });
                Settings settingsFromFile = ParameterReader.GetParameters<Settings>(settingsFromFileDictionary);
                settings = SettingsProvider.Create(parameters, settingsFromFile);
            }
            catch (Exception e)
            {
                WriteUserLine($"Failure reading a settings file. {e.Message}");
                return (int)ReturnValue.SettingsReadingFailure;
            }

            try
            {
                var fileSearch = new Hashing.FileSearch(settings.FileLookupIncludeHidden);
                FileInfo decoderFile = new FileInfo(settings.Decoder);
                Algorithm hashAlgorithm = ExecutionParameterResolver.ParseHashAlgorithmOrGetDefault(settings.HashAlgorithm);
                bool continueOnError = settings.FailOnError.HasValue ? !settings.FailOnError.Value : continueOnErrorDefault;
                IList<FileInfo> inputFiles = ExecutionParameterResolver.GetInputFiles(settings, fileSearch);

                if (!inputFiles.Any())
                    throw new InputFileMissingException("No files provided/found");

                WriteUserLine($"Hash algorithm: {hashAlgorithm}");

                var cancellation = new CancellationTokenSource();
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    cancellation.Cancel();
                };

                var processRunner = new ExternalProcess.ProcessRunner(
                    timeoutSec: settings.ProcessTimeoutSec ?? processTimeoutSecDefault,
                    exitTimeoutMs: settings.ProcessExitTimeoutMs ?? processExitTimeoutMsDefault,
                    startWaitMs: settings.ProcessStartWaitMs ?? processStartWaitMsDefault,
                    printProcessProgress);

                Audio.IAudioFileDecoder decoder = AudioDecoderFactory.Build(decoderFile, processRunner, settings.DecoderParameters);

                if (isVerification)
                {
                    var @params = new Verification.HashfileParams
                    {
                        HashFile = settings.HashFile,
                        InputDirectory = settings.InputDirectory,
                        HashfileEntrySeparator = settings.HashfileEntrySeparator,
                        HashfileExtensions = settings.HashfileExtensions
                    };
                    Verification.Verify(inputFiles, @params, decoder, continueOnError, hashAlgorithm, fileSearch, cancellation.Token);
                }
                else
                {
                    Computation.ComputeHashes(inputFiles, settings.OutputFormat, decoder, continueOnError, printProcessProgress, hashAlgorithm, cancellation.Token);
                }
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
            catch (OperationCanceledException e)
            {
                WriteUserLine("The operation was cancelled");
                return (int)ReturnValue.Cancellation;
            }
            catch (ExecutionException e)
            {
                WriteUserLine($"Process exited with code {e.ExitCode}");
                if (!printProcessProgress)
                    WriteUserLine($"Process output:\n{e.ProcessErrorOutput}");
                return (int)ReturnValue.ExecutionFailure;
            }
            catch (InputFileMissingException e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.NoFilesToProcess;
            }
            catch (Exception e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.ExecutionFailure;
            }

            WriteUserLine("Done!");
            return (int)ReturnValue.Success;
        }

        static void WriteUserLine(string text)
        {
            if (printProcessProgress)
            {
                Console.Error.WriteLine("");
                Console.Error.WriteLine("");
            }
            Console.Error.WriteLine(text);
        }
    }
}