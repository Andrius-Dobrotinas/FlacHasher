using System;

namespace Andy.FlacHash.Application
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ConfigurationAspectAttribute : Attribute
    {
        public string Name { get; }

        public ConfigurationAspectAttribute(string name)
        {
            Name = name;
        }
    }
}
