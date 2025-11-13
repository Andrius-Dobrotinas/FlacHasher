using System;

namespace Andy.Cmd.Parameter
{
    /// <summary>
    /// For use with <see cref="string"/> only
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AllowWhitespaceAttribute : Attribute
    {
    }
}
