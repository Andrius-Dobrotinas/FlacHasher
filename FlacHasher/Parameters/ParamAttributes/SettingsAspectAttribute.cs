using System;

namespace Andy.FlacHash.Application
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class SettingsAspectAttribute : Attribute
    {
        public string Name { get; }

        public SettingsAspectAttribute(string name)
        {
            Name = name;
        }
    }
}
