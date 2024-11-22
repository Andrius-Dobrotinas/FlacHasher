﻿using Andy.Cmd;
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
        const string settingsFileName = "settings.cfg";
        static bool printProcessProgress = false;
        static string HelpMessage = $"For help, use \"{CmdlineParameterNames.ModeHelp}\" option";

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

                if (initialCmdlineParams.IsHelp)
                {
                    PrintHelp(initialCmdlineParams.IsVerification);
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

                var paramTypes = initialCmdlineParams.IsVerification
                    ? new[] { typeof(VerificationParameters), typeof(InitialParams) }
                    : new[] { typeof(HashingParameters), typeof(InitialParams) };
                ParameterReader.ThrowOnUnexpectedArguments<CmdLineParameterAttribute>(argumentDictionary.Keys, paramTypes, caseInsensitive: lowercaseParams);

                var allParams = argumentDictionary.Concat(settingsFileParams)
                    .ToDictionary(x => x.Key, x => x.Value);

                settings = initialCmdlineParams.IsVerification
                    ? parameterReader.GetParameters<VerificationParameters>(allParams, inLowercase: lowercaseParams)
                    : parameterReader.GetParameters<HashingParameters>(allParams, inLowercase: lowercaseParams);
            }
            catch (ParameterMissingException e)
            {
                var property = Help.GetParameterMetadata(e.ParameterProperty.DeclaringType, e.ParameterProperty);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Provide the configuration for \"{property.DisplayName}\" via:");

                var groupedBySourceType = property.Sources.GroupBy(x => x.Value, x => x.Key);
                foreach (var p in groupedBySourceType.Select(x => new { SourceType = x.Key, ParamsString = string.Join(", ", x.Select(i => $"\"{i}\"")) }))
                    sb.AppendLine($"- {p.SourceType}: {p.ParamsString}");

                WriteUserLine(e.Message);
                WriteUserLine(sb.ToString());
                WriteUserLine(HelpMessage);
                return (int)ReturnValue.ArgumentNotProvided;
            }
            catch (ParameterGroupException e)
            {
                var parameterMetadata = e.Parameters.Select(x => Help.GetParameterMetadata(x.DeclaringType, x));
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

                if (initialCmdlineParams.IsVerification)
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
            if (isVerification)
            {
                PrintParameters<VerificationParameters>();
            }
            else
            {
                WriteUserLine("Hashing:");
                PrintParameters<VerificationParameters>();

                WriteUserLine("");
                WriteUserLine("Verification:");
                PrintParameters<VerificationParameters>();
            }

            WriteUserLine($"Settings file: {settingsFileName}");
        }

        static void PrintParameters<T>()
        {
            var properties = Help.GetAllParameterMetadata<T>();
            var paramterGroups = Help.GetAllParameterGroups<T>();
            var withDependencies = Help.GetDependencyDictionary(properties);

            var sb = new System.Text.StringBuilder();

            if (paramterGroups.Any())
                sb.AppendLine("=== Parameter groups ===");

            foreach (var group in paramterGroups)
            {
                sb.Append($"- \"{group.Key.Item2}\" -- {GetGroupingDescription(group.Key.Item1)}: ");
                sb.AppendLine(string.Join(", ", group.Select(item => $"{properties[item].DisplayName}")));

                foreach (var property in group) {
                    var propertyMetadata = properties[property];
                    PrintParameterDetails(sb, property, propertyMetadata, withDependencies, false, 1);
                };

                WriteUserLine(sb.ToString());
                sb.Clear();
            }

            WriteUserLine("");

            if (paramterGroups.Any())
                sb.AppendLine("=== Discrete parameters ===");

            var doneProperties = paramterGroups.SelectMany(x => x).ToList();
            var unlistedProperties = properties.Where(x => !doneProperties.Contains(x.Key));
            foreach (var (property, metadata) in unlistedProperties.OrderBy(x => x.Value.Optionality))
            {
                PrintParameterDetails(sb, property, metadata, withDependencies, true, 0);
            }
        }

        static void PrintParameterDetails(System.Text.StringBuilder sb, PropertyInfo property, ParameterMetadata metadata, Dictionary<PropertyInfo, ParameterMetadata[]> withDependencies, bool showOptionality, int baseIndentationLevel)
        {
            Indent(sb, baseIndentationLevel);
            sb.Append($"- {metadata.DisplayName}");
            if (metadata.Optionality != OptionalityMode.Mandatory)
            {
                var defaultValue = metadata.DefaultValue == null
                    ? null
                    : metadata.DefaultValue is Array
                        ? "[ " + string.Join("; ", (metadata.DefaultValue as IList<string>).Select(x => $"\"{x}\"")) + " ]"
                        : $"\"{metadata.DefaultValue}\"";

                if (showOptionality)
                {
                    sb.Append($" [{metadata.Optionality}");

                    if (defaultValue != null)
                        sb.Append($", default value: {defaultValue}");

                    sb.Append("] ");
                }
                else
                {
                    if (defaultValue != null)
                        sb.Append($" [default value: {defaultValue}] ");
                }
            }

            if (metadata.EmptyAllowed)
                sb.Append($"[Empty value allowed]");

            if (!string.IsNullOrEmpty(metadata.Description))
                sb.Append($": {metadata.Description}");

            sb.AppendLine($". Configured via:");
            foreach (var src in metadata.Sources.Select(x => $"{x.Key} [{x.Value}]"))
            {
                Indent(sb, baseIndentationLevel + 1);
                sb.AppendLine($"* {src}");
            }

            if (withDependencies.ContainsKey(property))
            {
                var dependendyString = string.Join(", ", withDependencies[property].Select(x => $"\"{x.DisplayName}\""));

                Indent(sb, baseIndentationLevel + 1);
                sb.AppendLine($"Requires: {dependendyString}");
            }

            WriteUserLine(sb.ToString());
            sb.Clear();
        }

        static void Indent(System.Text.StringBuilder sb, int level)
        {
            for (int i = 0; i < level; i++)
                sb.Append("  ");
        }

        static string GetGroupingDescription(Type type)
        {
            if (type == typeof(AtLeastOneOfAttribute))
                return "At least one of the following must have a value";

            if (type == typeof(EitherOrAttribute))
                return "Strictly One of the following must have a value";

            if (type == typeof(OptionalEitherOrAttribute))
                return "Optional - no more than one of the following must have a value";

            return "";
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