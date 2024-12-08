using System;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParameterGroupException : ParameterException
    {
        public string GroupName { get; }
        public PropertyInfo[] Parameters { get; }

        public ParameterGroupException(string message, string groupName, PropertyInfo[] @params)
            : base($"{message}: {groupName}")
        {
            GroupName = groupName;
            Parameters = @params;
        }
    }
}
