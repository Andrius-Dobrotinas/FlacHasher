using Andy.Cmd;
using Andy.Cmd.Parameter;
using Andy.ExternalProcess;
using Andy.FlacHash.Application.Audio;
using Andy.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Andy.FlacHash.Application.Cmd
{
    public class Program
    {
        const string settingsFileName = "settings.ini";
        static bool printProcessProgress = false;
        static string HelpMessage = $"For info on how to use this, run \"{CmdlineParameterNames.ModeHelp}\" command";

        static int Main(string[] args)
        {
            bool lowercaseParams = true;
            InitialParams initialCmdlineParams;
            MasterParameters settings;
            
            var parameterReader = ParameterReader.Build();
            try
            {
                var argumentDictionary = ArgumentSplitter.GetArguments(args, paramNamesToLowercase: lowercaseParams);
                initialCmdlineParams = parameterReader.GetParameters<InitialParams>(argumentDictionary, inLowercase: lowercaseParams);

                if (!argumentDictionary.Any())
                {
                    WriteUserLine(HelpMessage);
                    return (int)ReturnValue.ArgumentNotProvided;
                }

                if (initialCmdlineParams.IsHelp)
                {
                    PrintHelp(initialCmdlineParams.IsVerification ?? false);
                    return 0;
                }

                IDictionary<string, string[]> settingsFileParams;
                try
                {
                    var settingsFile = new FileInfo(settingsFileName);
                    settingsFileParams = SettingsFile.ReadIniFile(settingsFile, initialCmdlineParams.Profile, initialCmdlineParams.DecoderProfile, initialCmdlineParams.HashingProfile)
                        .ToDictionary(x => lowercaseParams ? x.Key.ToLowerInvariant() : x.Key, x => new[] { x.Value });
                }
                catch (Exception e)
                {
                    WriteUserLine($"Failure reading a settings file. {e.Message}");
                    return (int)ReturnValue.SettingsReadingFailure;
                }

                var paramTypes = initialCmdlineParams.IsVerification ?? false
                    ? new[] { typeof(VerificationParameters), typeof(InitialParams) }
                    : new[] { typeof(HashingParameters), typeof(InitialParams) };
                ParameterReader.ThrowOnUnexpectedArguments<CmdLineParameterAttribute>(argumentDictionary.Keys, paramTypes, caseInsensitive: lowercaseParams);

                var allParams = argumentDictionary.Concat(settingsFileParams)
                    .ToDictionary(x => x.Key, x => x.Value);

                settings = initialCmdlineParams.IsVerification ?? false
                    ? parameterReader.GetParameters<VerificationParameters>(allParams, inLowercase: lowercaseParams)
                    : parameterReader.GetParameters<HashingParameters>(allParams, inLowercase: lowercaseParams);
            }
            catch (ParameterMissingException e)
            {
                var property = Metadata.GetParameterMetadata<ParameterAttribute>(e.ParameterProperty.DeclaringType, e.ParameterProperty);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine(e.Message);
                sb.AppendLine();
                sb.AppendLine($"Provide the configuration for \"{property.DisplayName}\" via:");
                sb.AppendLine(string.Join(", ", property.Sources.Select(x => $"{x.Name}")));
                sb.AppendLine();
                sb.AppendLine(HelpMessage);
                WriteUserLine(sb.ToString());
                return (int)ReturnValue.ArgumentNotProvided;
            }
            catch (ParameterGroupException e)
            {
                var parameterMetadata = e.Parameters.Select(x => Metadata.GetParameterMetadata<ParameterAttribute>(x.DeclaringType, x));
                var paramsString = string.Join(", ", parameterMetadata.Select(x => x.DisplayName));
                WriteUserLine(e.Message);
                WriteUserLine($"Relevant parameters: {paramsString}");
                WriteUserLine(HelpMessage);
                return (int)ReturnValue.ArgumentError;
            }
            catch (ParameterException e)
            {
                WriteUserLine(e.Message);
                WriteUserLine(HelpMessage);
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

                FileInfo decoderFile = AudioDecoder.ResolveDecoderOrThrow(settings.DecoderExe);

                WriteUserLine($"Hash algorithm: {settings.HashAlgorithm}");

                var processRunner = new ExternalProcess.ProcessRunner(
                    timeoutSec: settings.ProcessTimeoutSec,
                    exitTimeoutMs: settings.ProcessExitTimeoutMs,
                    startWaitMs: settings.ProcessStartWaitMs,
                    printProcessProgress);

                var decoderParams = AudioDecoder.GetDefaultDecoderParametersIfEmpty(settings.DecoderParameters, decoderFile);
                FlacHash.Audio.IAudioFileDecoder decoder = AudioDecoder.Build(decoderFile, processRunner, decoderParams);
                var fileSearch = new FileSearch(settings.FileLookupIncludeHidden);

                if (initialCmdlineParams.IsVerification ?? false)
                {
                    Verification.Verify((VerificationParameters)settings, decoder, fileSearch, cancellation.Token);
                }
                else
                {
                    Hashing.ComputeHashes(decoder, (HashingParameters)settings, printProcessProgress, fileSearch, cancellation.Token);
                }
            }
            catch (ConfigurationException e)
            {
                WriteUserLine(e.Message);
                return (int)ReturnValue.ConfigurationError;
            }
            catch (FlacHash.Audio.IOException e)
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

        static void PrintHelp(bool isVerification)
        {
            var helpText = Help.GetHelpText();
            var helpHashfileText = Help.GetHashfileHelpText();
            var helpCmdText = Help.GetTextResource(Assembly.GetExecutingAssembly(), "help.txt");

            helpText = helpText.Replace(Help.Placeholder.ApplicationSpecific, helpCmdText);
            helpText = helpText.Replace(Help.Placeholder.HashfileDescription, helpHashfileText);

            var linesHelp = helpText.Split(Environment.NewLine).ToArray();

            var (sharedDecoderProperties, sharedOpSpecificProperties, sharedMiscProperties) = Help.GetPropertiesByParameterPurpose<MasterParameters>();
            var sharedParamProperties = sharedDecoderProperties.Concat(sharedMiscProperties).ToList();

            foreach (var line in linesHelp)
            {
                if (line == "{HASHING_PARAMS}")
                {
                    var @paramsLine = Help.GetOperationAndMiscParameterString<HashingParameters, ParameterAttribute>(sharedParamProperties);
                    WriteUserLine(@paramsLine);
                }
                else if (line == "{VERIFICATION_PARAMS}")
                {
                    var @paramsLine = Help.GetOperationAndMiscParameterString<VerificationParameters, ParameterAttribute>(sharedParamProperties);
                    WriteUserLine(@paramsLine);
                }
                else if (line == "{DECODER_PARAMS}")
                {
                    var paramsLine = Help.GetParameterString<MasterParameters, ParameterAttribute>(sharedDecoderProperties, sharedMiscProperties);
                    WriteUserLine(paramsLine);
                }
                else
                {
                    WriteUserLine(line);
                }
            }
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