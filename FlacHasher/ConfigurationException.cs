using System;

namespace Andy.FlacHash
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message)
            : base(message)
        {

        }
    }
}