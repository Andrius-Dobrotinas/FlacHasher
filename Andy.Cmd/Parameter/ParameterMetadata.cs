﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public enum OptionalityMode
    {
        Mandatory = 0,
        Conditional = 1,
        Optional = 2
    }

    public class ParameterMetadata
    {
        public PropertyInfo Property { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ICollection<ParameterAttribute> Sources { get; set; }
        public OptionalityMode Optionality { get; set; }
        public bool EmptyAllowed { get; set; }
        public object DefaultValue { get; set; }
        public PropertyInfo[] RequiredWith { get; set; }
    }
}