using FlacHasher.CommandLine;
using FlacHasher.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlacHasher
{
    class Program
    {
        static int Main(string[] args)
        {
            var argDictionary = ParseArguments(args);

            Parameters arguments;
            try
            {
                arguments = ParameterReader.GetParameters(argDictionary);
            }
            catch (Exception e) //TODO: the type
            {
                Console.Error.WriteLine(e.Message);
                return -1;
            }

            var decoder = new CmdLineFlacDecoder(arguments.Decoder);
            var hasher = new FileHasher(decoder, new Sha256HashComputer());

            byte[] hash = hasher.ComputerHash(arguments.InputFile);

            var formatTheHash = arguments.FormatOutput;

            OutputHash(hash, formatTheHash);

            Console.Error.WriteLine("Done!");

            return 0;
        }

        private static void OutputHash(byte[] hash, bool formatTheHash)
        {
            if (formatTheHash)
            {
                Console.OpenStandardOutput().Write(hash, 0, hash.Length);
                Console.Error.WriteLine();
            }
            else
            {
                Console.WriteLine(BitConverter.ToString(hash));
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