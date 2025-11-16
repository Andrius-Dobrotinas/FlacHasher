using System;

namespace Andy.FlacHash.Application
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ConfigurationFacetAttribute : Attribute
    {
        public string Name { get; }

        public ConfigurationFacetAttribute(string name)
        {
            Name = name;
        }
    }
}
