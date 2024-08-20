using System;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterAttribute : Attribute
    {
        public string Name { get; set; }

        public ParameterAttribute(string name)
        {
            Name = name;
        }
    }
}