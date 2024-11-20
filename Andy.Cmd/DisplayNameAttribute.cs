using System;

namespace Andy.Cmd
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public DisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}