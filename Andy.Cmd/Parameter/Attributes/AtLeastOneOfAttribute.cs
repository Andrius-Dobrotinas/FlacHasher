﻿using System;

namespace Andy.Cmd.Parameter
{
    /// <summary>
    /// Specifies a group of parameters (identified by <see cref="AtLeastOneOfAttribute.GroupKey"/>)
    /// out of which At Least One parameter is required to have a value.
    /// This means that the group is Required to have a value.
    /// A <see cref="ParameterGroupException"/> gets thrown no parameter has a value.
    /// 
    /// Paramteres of different types can belong to the same group.
    /// 
    /// One parameter can belong to multiple groups.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class AtLeastOneOfAttribute : OptionalAttribute
    {
        /// <summary>
        /// Ties parameters to the condition.
        /// All parameters with a given key are part of the condition.
        /// </summary>
        public string GroupKey { get; }

        public AtLeastOneOfAttribute(string groupKey)
        {
            if (string.IsNullOrWhiteSpace(groupKey))
                throw new ArgumentException("A non-empty value is required", nameof(groupKey));
            GroupKey = groupKey;
        }
    }
}