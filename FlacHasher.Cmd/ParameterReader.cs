using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.Cmd
{
    public class ParameterReader
    {
        /// <summary>
        /// Properties of a returned object will only have Null values if the parameters are not specified. If a parameter is specified with no value, a corresponding property will be an empty string
        /// </summary>
        public static Parameters GetParameters(IDictionary<string, string> arguments)
        {
            return new Parameters
            {
                Decoder = GetValue(arguments, ArgumentNames.Decoder),
                InputFiles = GetValue(arguments, ArgumentNames.InputFiles, paths => paths.Split(';')),
                InputDirectory = GetValue(arguments, ArgumentNames.Decoder),
                TargetFileExtension = GetValue(arguments, ArgumentNames.FileExtension),
                OutputFormat = GetValue(arguments, ArgumentNames.FileExtension)
            };
        }

        private static T GetValue<T>(IDictionary<string, string> arguments, string argName, Func<string, T> createAnInstance)
        {
            string value = GetValue(arguments, argName);

            return String.IsNullOrEmpty(value)
                ? default(T)
                : createAnInstance(value);
        }

        private static string GetValue(IDictionary<string, string> arguments, string argName)
        {
            string value;

            return arguments.TryGetValue(argName, out value)
                ? value ?? ""
                : null;
        }

        /// <summary>
        /// Throws an exception if file/directory doesn't exist
        /// </summary>
        private static bool IsDirectory(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }
    }
}