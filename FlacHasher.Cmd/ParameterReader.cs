using System;
using System.Collections.Generic;
using System.IO;

namespace FlacHasher
{
    public class ParameterReader
    {
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            if (!arguments.ContainsKey(ArgumentNames.Decoder))
            {
                // TODO: exception type
                throw new Exception($"The decoder executable file has not been specified. Use {ArgumentNames.Decoder}= option");
            }

            var decoderPath = arguments[ArgumentNames.Decoder];
            var decoder = new FileInfo(decoderPath);

            if (!arguments.ContainsKey(ArgumentNames.Input))
            {
                // TODO: exception type
                throw new Exception($"The input file has not been specified. Use {ArgumentNames.Input}= option");
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