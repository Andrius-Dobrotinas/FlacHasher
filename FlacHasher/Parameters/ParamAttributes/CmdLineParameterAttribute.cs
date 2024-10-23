using Andy.Cmd.Parameter;
using System;
using System.Collections.Generic;

namespace Andy.FlacHash.Application
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CmdLineParameterAttribute : ParameterAttribute
    {
        public CmdLineParameterAttribute(string name) : base(name)
        {
        }
    }
}