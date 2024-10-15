using Andy.Cmd.Parameter;
using System;

namespace Andy.FlacHash.Cmd
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IniEntryAttribute : ParameterAttribute
    {
        public IniEntryAttribute(string name) : base(name)
        {
        }
    }
}