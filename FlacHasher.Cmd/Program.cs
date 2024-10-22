using Andy.Cmd;
using Andy.Cmd.Parameter;
using Andy.ExternalProcess;
using Andy.FlacHash.Hashing.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Andy.FlacHash.Cmd
{
    public class Program
    {
        const string settingsFileName = "settings.cfg";
        static bool printProcessProgress = false;

        static int Main(string[] args)
        {
            bool lowercaseParams = true;
            InitialParams initialCmdlineParams;
            CmdApplicationParameters settings;
            VerificationSettings verificationSettings;
            
            var parameterReader = ParameterReader.Build();
            try
            {
            var argumentDictionary = ArgumentSplitter.GetArguments(args, paramNamesToLowercase: lowercaseParams);
                initialCmdlineParams = parameterReader.GetParameters<InitialParams>(argumentDictionary, inLowercase: lowercaseParams);

                IDictionary<string, string[]> settingsFileParams;
            try
            {
                    var settingsFile = new FileInfo(settingsFileName);
                    settingsFileParams = SettingsFile.ReadIniFile(settingsFile, initialCmdlineParams.Profile)
                        .ToDictionary(x => lowercaseParams ? x.Key.ToLowerInvariant() : x.Key, x => new[] { x.Value });
                }
                catch (Exception e)
                {
                    WriteUserLine($"Failure reading a settings file. {e.Message}");
                    return (int)ReturnValue.SettingsReadingFailure;
                }

                var paramTypes = initialCmdlineParams.IsVerification
                    ? new[] { typeof(CmdApplicationParameters), typeof(VerificationSettings), typeof(InitialParams) }
                    : new[] { typeof(CmdApplicationParameters), typeof(InitialParams) };
                ThrowOnUnexpectedArguments<CmdLineParameterAttribute>(argumentDictionary.Keys, paramTypes, paramNamesToLowercase: lowercaseParams);

                var allParams = argumentDictionary.Concat(settingsFileParams)
                    .ToDictionary(x => x.Key, x => x.Value);

                settings = parameterReader.GetParameters<CmdApplicationParameters>(allParams, inLowercase: lowercaseParams);
                verificationSettings = initialCmdlineParams.IsVerification
                    ? parameterReader.GetParameters<VerificationSettings>(allParams, inLowercase: lowercaseParams)
                    : null;

            }
            catch (ParameterMissingException e)
            {
                var attrs = e.ParameterProperty.GetCustomAttributes<ParameterAttribute>(false).Cast<ParameterAttribute>().ToList();
                var cmdlineAttr = attrs.FirstOrDefault(x => x is CmdLineParameterAttribute);
                if (cmdlineAttr != null)
                {
                    WriteUserLine($"{e.Message}. Specify the following parameter: {cmdlineAttr.Name}");
                }
                else if (attrs.FirstOrDefault(x => x is IniEntryAttribute) != null)
                {
                    var iniAttr = attrs.FirstOrDefault(x => x is IniEntryAttribute);
                    WriteUserLine($"{e.Message}. Specify the following item the settings file: {iniAttr}");
                }
                else
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
                // For console output, this is only relevant when the process is actually running
                printProcessProgress = settings.PrintDecoderProgress;

                var cancellation = new CancellationTokenSource();
                Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                {
                    e.Cancel = true;
                    cancellation.Cancel();
                };

                FileInfo decoderFile = ResolveDecoderOrThrow(settings);
                var fileSearch = new FlacHash.Hashing.FileSearch(settings.FileLookupIncludeHidden);
                IList<FileInfo> inputFiles = Functions.GetInputFiles(settings, fileSearch);
                if (!inputFiles.Any())
                    throw new InputFileMissingException("No files provided/found");

                Algorithm hashAlgorithm = settings.HashAlgorithm;
                bool continueOnError = !settings.FailOnError;
                WriteUserLine($"Hash algorithm: {hashAlgorithm}");

                var processRunner = new ExternalProcess.ProcessRunner(
                    timeoutSec: settings.ProcessTimeoutSec,
                    exitTimeoutMs: settings.ProcessExitTimeoutMs,
                    startWaitMs: settings.ProcessStartWaitMs,
                    printProcessProgress);

                var decoderParams = AudioDecoder.GetDefaultDecoderParametersIfEmpty(settings.DecoderParameters, decoderFile);
                Audio.IAudioFileDecoder decoder = AudioDecoder.Build(decoderFile, processRunner, decoderParams);

                if (initialCmdlineParams.IsVerification)
                {
                    Verification.Verify(inputFiles, verificationSettings, decoder, hashAlgorithm, fileSearch, cancellation.Token);
                }
                else
                {
                    Hashing.ComputeHashes(inputFiles, settings.OutputFormat, decoder, continueOnError, printProcessProgress, hashAlgorithm, cancellation.Token);
                }
            }
            catch (ConfigurationException e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.ConfigurationError;
            }
            catch (IOException e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.InputReadingFailure;
            }
            catch (OperationCanceledException)
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

        public static void ThrowOnUnexpectedArguments<TParamAttr>(IEnumerable<string> @paramsNames, Type[] paramClasses, bool paramNamesToLowercase = false)
            where TParamAttr : ParameterAttribute
        {
            var acceptedParamNames = paramClasses.SelectMany(x => x.GetProperties())
                .SelectMany(x => x.GetCustomAttributes<TParamAttr>())
                .Select(x => x.Name);

            if (paramNamesToLowercase)
                acceptedParamNames = acceptedParamNames.Select(x => x.ToLowerInvariant());

            if (paramNamesToLowercase)
                @paramsNames = @paramsNames.Select(x => x.ToLowerInvariant());
            
            var unexpectedParams = @paramsNames.Except(acceptedParamNames).ToList();
            if (unexpectedParams.Any())
                throw new ParameterException($"The following params are not accepted: {string.Join(',', unexpectedParams)}");
        }

        public static FileInfo ResolveDecoderOrThrow(ApplicationSettings settings)
        {
            return AudioDecoder.ResolveDecoder(settings.Decoder)
                ?? throw new ConfigurationException($"The specified decoder exe file was not found (not in PATH either): {settings.Decoder}");
        }
    }
}