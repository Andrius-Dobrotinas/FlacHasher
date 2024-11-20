using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    public class ParameterDescription
    {
        public PropertyInfo Property { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ICollection<KeyValuePair<string, string>> Sources { get; set; }
        public bool IsOptional { get; set; }
        public PropertyInfo RequiredWith { get; set; }
    }
}