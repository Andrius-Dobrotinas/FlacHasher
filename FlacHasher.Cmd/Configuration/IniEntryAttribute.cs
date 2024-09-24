using System;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IniEntryAttribute : ParameterAttribute
    {
        public IniEntryAttribute(string name) : base(name)
        {
        }
    }
}