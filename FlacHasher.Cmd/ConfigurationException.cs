using System;

namespace Andy.FlacHash.Cmd
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message)
            : base(message)
        {

        }
    }
}