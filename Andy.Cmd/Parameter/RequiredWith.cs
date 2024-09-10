using System;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RequiredWith : OptionalAttribute
    {
        /// <summary>
        /// When this property has a value, the target property has to have one too
        /// </summary>
        public string OtherPropertyName { get; set; }

        public RequiredWith(string otherPropertyName)
        {
            if (string.IsNullOrWhiteSpace(otherPropertyName))
                throw new ArgumentException("A non-empty value is required", nameof(otherPropertyName));
            OtherPropertyName = otherPropertyName;
        }
    }
}