using System;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ParameterAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; set; }

        public ParameterAttribute(string name)
        {
            Name = name;
        }
    }
}