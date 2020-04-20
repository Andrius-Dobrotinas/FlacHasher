using System;

namespace Andy.Configuration.Ini
{
    public class InvalidSectionDeclaration : Exception
    {
        public string Value { get; }

        public InvalidSectionDeclaration(string value)
            : base($"Section declaration is invalid. Check {nameof(Value)} property.")
        {
            Value = value;
        }
    }
}