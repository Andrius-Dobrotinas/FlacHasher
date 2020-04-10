using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Cmd
{
    public static class ExecutionParameterResolver
    {
        /// <summary>
        /// Returns user-provider value if one was provided. Otherwise returns one from the settings file.
        /// Empty strings are treated as valid values that indicate not to use formatting (ie equal Null).
        /// </summary>
        public static string ResolveOutputFormat(Settings settings, Parameters cmdlineArguments)
        {
            if (cmdlineArguments.OutputFormat != null)
            {
                if (cmdlineArguments.OutputFormat == "")
                    return null;
                return cmdlineArguments.OutputFormat;
            }

            if (settings.OutputFormat != null)
            {
                if (settings.OutputFormat == "")
                    return null;
                return settings.OutputFormat;
            }

            return null;
        }
    }
}