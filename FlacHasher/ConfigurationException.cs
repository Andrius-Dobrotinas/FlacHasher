using System;

namespace Andy.FlacHash.Application
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message)
            : base(message)
        {

        }
    }
}