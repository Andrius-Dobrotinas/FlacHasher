using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Andy.Cmd.Parameter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ParameterDescriptionAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public ParameterDescriptionAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public ParameterDescriptionAttribute(string description)
        {
            Description = description;
        }

        public static string GetDescription<T>(Expression<Func<T, object>> propertySelector)
        {
            if (propertySelector == null)
                return null;

            var memberExpression = propertySelector.Body as MemberExpression;
            if (memberExpression == null && propertySelector.Body is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression?.Member is PropertyInfo property)
            {
                var attribute = property.GetCustomAttribute<ParameterDescriptionAttribute>(false);
                return attribute?.Description;
            }

            return null;
        }
    }
}