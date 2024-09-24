using System;
using System.Collections.Generic;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CmdLineParameterAttribute : ParameterAttribute
    {
        public CmdLineParameterAttribute(string name) : base(name)
        {
        }
    }
}