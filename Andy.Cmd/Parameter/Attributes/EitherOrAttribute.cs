using System;

namespace Andy.Cmd.Parameter
{
    // TODO: multiple could be allowed, but that would require more development and I don't need that now

    /// <summary>
    /// Specifies a group of parameters (identified by <see cref="EitherOrAttribute.GroupKey"/>)
    /// out of which Exactly One parameter is required to have a value.
    /// This means that the group is Required to have a value, and it can only come from one paramater.
    /// A <see cref="ParameterGroupException"/> gets thrown if more than one parameter has a value or if none do.
    /// 
    /// Paramteres of different types can belong to the same group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EitherOrAttribute : OptionalAttribute
    {
        /// <summary>
        /// Ties parameters to the "either/or" condition.
        /// All parameters with a given key are part of the condition.
        /// </summary>
        public string GroupKey { get; }

        public EitherOrAttribute(string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey))
                throw new ArgumentException("A non-empty value is required", nameof(groupKey));
            GroupKey = groupKey;
        }
    }
}