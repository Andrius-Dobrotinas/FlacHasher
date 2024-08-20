using System;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionalAttribute : Attribute
    {
        /// <summary>
        /// No applicable to arrays
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
