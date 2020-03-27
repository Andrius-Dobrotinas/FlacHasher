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
        static int Main(string[] args)
        {
            var arguments = ParseArguments(args);

            Parameters parameters;
            try
            {
                parameters = ParameterReader.GetParameters(arguments);
            }
            catch (CmdLineArgNotFoundException e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }

            var decoder = new Input.Flac.CmdLineDecoder(parameters.Decoder);
            var hasher = new FileHasher(decoder, new Sha256HashComputer());
            var multiHasher = new MultipleFileHasher(hasher);

            var hashes = multiHasher.ComputeHashes(parameters.InputFiles);

            foreach(var entry in hashes)
            {
                OutputHash(entry.Hash, parameters.OutputFormat, entry.File);
            };

            Console.Error.WriteLine("Done!");

            return 0;
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

        private static IDictionary<string, string> ParseArguments(string[] args)
        {
            var argParser = new ArgumentParser();

            return args.Select(argParser.ParseArgument)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value);
        }
    }
}