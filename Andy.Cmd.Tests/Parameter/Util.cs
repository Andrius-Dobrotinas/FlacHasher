using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;

namespace Andy.Cmd.Parameter
{
    internal class Util
    {
        public static void Set_ParameterValueResolver_Up<TParams>(Mock<IParameterValueResolver> resolver, PropertyInfo property, object value)
        {
            resolver.Setup(
                x => x.ReadParameter<TParams>(
                    It.Is<PropertyInfo>(
                        arg => arg == property),
                    It.IsAny<IDictionary<string, string[]>>(),
                    It.IsAny<TParams>(),
                    It.IsAny<bool>()))
                .Callback<PropertyInfo, IDictionary<string, string[]>, TParams, bool>(
                    (property, b, instance, d) => property.SetValue(instance, value));
        }
    }
}
