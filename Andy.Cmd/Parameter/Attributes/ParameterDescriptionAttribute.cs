using System;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterDescriptionAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public ParameterDescriptionAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public ParameterDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}