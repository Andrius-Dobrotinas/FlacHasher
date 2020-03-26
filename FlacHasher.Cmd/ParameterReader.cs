using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Cmd
{
    public class ParameterReader
    {
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey(ArgumentNames.Decoder))
            {
                throw new CmdLineArgNotFoundException($"The decoder executable file has not been specified. Use {ArgumentNames.Decoder}= option");
            }

            var decoderPath = arguments[ArgumentNames.Decoder];
            var decoder = new FileInfo(decoderPath);

            if (!arguments.ContainsKey(ArgumentNames.Input))
            {
                throw new CmdLineArgNotFoundException($"The input file has not been specified. Use {ArgumentNames.Input}= option");
            }

            var inputFilePath = arguments[ArgumentNames.Input];
            var inputFile = new FileInfo(inputFilePath);

            var args = new Parameters
            {
                Decoder = decoder,
                InputFile = inputFile,
            };

            if (arguments.ContainsKey(ArgumentNames.OutputFormat))
            {
                args.OutputFormat = arguments[ArgumentNames.OutputFormat];
            }

            return args;
        }
    }
}