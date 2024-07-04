using Andy.Cmd;
using Andy.FlacHash.ExternalProcess;
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
            bool isVerification;
            try
            {
                var argumentDictionary = ArgumentSplitter.GetArguments(args);
                isVerification = argumentDictionary.ContainsKey(ArgumentNames.ModeVerify);
                parameters = ParameterReader.GetParameters(argumentDictionary);
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

            try
            {
                var fileSearch = new FileSearch(settings.FileLookupIncludeHidden);
                FileInfo decoderFile = ExecutionParameterResolver.GetDecoder(settings, parameters);
                string outputFomat = ExecutionParameterResolver.ResolveOutputFormat(settings, parameters);
                int processExitTimeoutMs = ExecutionParameterResolver.GetProcessExitTimeoutInMs(settings, parameters, processExitTimeoutMsDefault);
                int processTimeoutSec = ExecutionParameterResolver.GetProcessTimeoutInSeconds(settings, parameters, processTimeoutSecDefault);
                int processStartWaitMs = settings.ProcessStartWaitMs ?? processStartWaitMsDefault;
                bool continueOnError = ExecutionParameterResolver.GetContinueOnError(settings, parameters, continueOnErrorDefault);
                IList<FileInfo> inputFiles = ExecutionParameterResolver.GetInputFiles(parameters, fileSearch);

                if (!inputFiles.Any())
                    throw new InputFileMissingException("No files provided/found");

                var cancellation = new CancellationTokenSource();
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    cancellation.Cancel();
                };

                var processRunner = new ExternalProcess.ProcessRunner(
                    settings.ProcessTimeoutSec ?? processTimeoutSecDefault,
                    settings.ProcessExitTimeoutMs ?? processExitTimeoutMsDefault,
                    settings.ProcessStartWaitMs ?? processStartWaitMsDefault,
                    printProcessProgress);

                if (isVerification)
                {
                    Verification.Verify(inputFiles, parameters, decoderFile, processRunner, continueOnError, settings.HashfileExtensions, fileSearch, cancellation.Token);
                }
                else
                {
                    Computation.ComputeHashes(inputFiles, outputFomat, decoderFile, processRunner, continueOnError, printProcessProgress, cancellation.Token);
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