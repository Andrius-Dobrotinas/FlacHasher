using System;

namespace Andy.Cmd.Parameter
{
    // TODO: multiple could be allowed, but that would require more development and I don't need that now
    /// <summary>
    /// Provides a way to specify a group of parameters out of which only one paramater has to have a value.
    /// Only one of the parameters marked with the same <see cref="EitherOrAttribute.GroupKey"/> can have a value and
    /// One of the parameters marked with the same <see cref="EitherOrAttribute.GroupKey"/> MUST have a value.
    /// A <see cref="ParameterGroupException"/> gets thrown if more than one parameter has value.
    /// 
    /// Is only applicable to <see cref="string"/> and <see cref="string[]"/> properties.
    /// Paramteres of different allowed types can belong to the same group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EitherOrAttribute : Attribute
    {
        /// <summary>
        /// Ties parameters to the "either/or" condition.
        /// All parameters with a given key are part of the condition.
        /// </summary>
        public string GroupKey { get; set; }

        public EitherOrAttribute(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A non-empty value is required", nameof(key));
            GroupKey = key;
        }
    }
}