﻿using System;

namespace Andy.Cmd.Parameter
{
    /// <summary>
    /// Specifies a group of parameters (identified by <see cref="EitherOrAttribute.GroupKey"/>)
    /// out of which No More Than One parameter is required to have a value.
    /// It's allowed for the group not to have a value at all.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionalEitherOrAttribute : EitherOrAttribute
    {
        public OptionalEitherOrAttribute(string groupKey) : base(groupKey)
        {
        }
    }
}