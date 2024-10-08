using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Andy.Cmd.Parameter.ParameterReader_Tests.ParameterValueResolver_Tests.General;

namespace Andy.Cmd.Parameter.ParameterReader_Tests.ParameterValueResolver_Tests
{
    public class AllowEmptyAttribute_Tests
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase(nameof(TestParams_Array.OptionalEmptyAllowed), "")]
        [TestCase(nameof(TestParams_Array.OptionalEmptyAllowed), " ")]
        [TestCase(nameof(TestParams_Array.OptionalEmptyAllowed), "\t")]
        [TestCase(nameof(TestParams_Array.EmptyAllowed), "")]
        [TestCase(nameof(TestParams_Array.EmptyAllowed), " ")]
        [TestCase(nameof(TestParams_Array.EmptyAllowed), "\t")]
        public void Array__When_ValueIs_EmptyStringOrWhitespace__Must_Return_EmptyArray(string propertyName, string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var prop = typeof(TestParams_Array).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var result = new TestParams_Array();
            target.ReadParameter(prop, argvs, result);

            var resutValue = prop.GetValue(result);
            Assert.IsNotNull(resutValue);
            Assert.IsEmpty(resutValue as string[]);
        }

        [TestCase(nameof(TestParamsString.RegularEmptyAllowed), "")]
        [TestCase(nameof(TestParamsString.RegularEmptyAllowed), " ")]
        [TestCase(nameof(TestParamsString.RegularEmptyAllowed), "\t")]
        [TestCase(nameof(TestParamsString.OptionalEmptyAllowed), "")]
        [TestCase(nameof(TestParamsString.OptionalEmptyAllowed), " ")]
        [TestCase(nameof(TestParamsString.OptionalEmptyAllowed), "\t")]
        [TestCase(nameof(TestParamsString.OptionalDefaultValue_EmptyAllowed), "")]
        [TestCase(nameof(TestParamsString.OptionalDefaultValue_EmptyAllowed), " ")]
        [TestCase(nameof(TestParamsString.OptionalDefaultValue_EmptyAllowed), "\t")]
        public void String__ValueIs_EmptyStringOrWhitespace__Must_Return_EmptyString(string propertyName, string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new string[] { value } }
            };
            var result = new TestParamsString();
            var prop = typeof(TestParamsString).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual("", prop.GetValue(result));
        }

        [TestCase(nameof(TestParamsString.RegularEmptyAllowed))]
        public void String__Mandatory_ParameterPresent_But_Value_NotProvided__Must_Reject(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg-other", new string[] { "value" } }
            };
            var result = new TestParamsString();
            var prop = typeof(TestParamsString).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<ParameterMissingException>(() => target.ReadParameter(prop, argvs, result));
        }

        // TODO: this should be enforced at a higher level
        [TestCase(nameof(TestParams2.Primitive), "")]
        [TestCase(nameof(TestParams2.Primitive), null)]
        [TestCase(nameof(TestParams2.Primitive_Optional), "")]
        [TestCase(nameof(TestParams2.Primitive_Optional), null)]
        [TestCase(nameof(TestParams2.Nullable_Primitive), "")]
        [TestCase(nameof(TestParams2.Nullable_Primitive), null)]
        [TestCase(nameof(TestParams2.Primitive_Bool), "")]
        [TestCase(nameof(TestParams2.Primitive_Bool), null)]
        [TestCase(nameof(TestParams2.Primitive_Bool_Optional), "")]
        [TestCase(nameof(TestParams2.Primitive_Bool_Optional), null)]
        [TestCase(nameof(TestParams2.Nullable_Primitive_Bool), "")]
        [TestCase(nameof(TestParams2.Nullable_Primitive_Bool), null)]
        [TestCase(nameof(TestParams2.Enum), null)]
        [TestCase(nameof(TestParams2.Enum), "")]
        [TestCase(nameof(TestParams2.Enum_Optional), null)]
        [TestCase(nameof(TestParams2.Enum_Optional), "")]
        [TestCase(nameof(TestParams2.Nullable_Enum), null)]
        [TestCase(nameof(TestParams2.Nullable_Enum), "")]
        public void NotSupported__On_Primitive_And_NullablePrimitive_And_Array_Types(string propertyName, string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new string[] { value } }
            };
            var result = new TestParams2();
            var prop = typeof(TestParams2).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<NotSupportedException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams2.Primitive))]
        [TestCase(nameof(TestParams2.Primitive_Optional))]
        [TestCase(nameof(TestParams2.Nullable_Primitive))]
        [TestCase(nameof(TestParams2.Primitive_Bool))]
        [TestCase(nameof(TestParams2.Primitive_Bool_Optional))]
        [TestCase(nameof(TestParams2.Nullable_Primitive_Bool))]
        public void NotSupported__On_Primitive_And_NullablePrimitive_Types__Even_When_Param_IsNotPresent(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>
            {
            };
            var result = new TestParams2();
            var prop = typeof(TestParams2).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<NotSupportedException>(() => target.ReadParameter(prop, argvs, result));
        }


        class TestParams_Array
        {
            [Parameter("arg")]
            public string[] Regular { get; set; }

            [Parameter("arg")]
            [Optional]
            public string[] Optional { get; set; }

            [Parameter("arg")]
            [AllowEmpty()]
            public string[] EmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty()]
            public string[] OptionalEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: new[] { "one" })]
            public string[] OptionalDefaultValue { get; set; }
        }

        class TestParamsString
        {
            [Parameter("arg")]
            [AllowEmpty()]
            public string RegularEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty()]
            public string OptionalEmptyAllowed { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: "x")]
            [AllowEmpty]
            public string OptionalDefaultValue_EmptyAllowed { get; set; }
        }

        class TestParams2
        {
            [Parameter("arg")]
            [AllowEmpty]
            public int Primitive { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public bool Primitive_Bool { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public int? Nullable_Primitive { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public bool? Nullable_Primitive_Bool { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty]
            public int Primitive_Optional { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty]
            public bool Primitive_Bool_Optional { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public TestEnum Enum { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public TestEnum? Nullable_Enum { get; set; }

            [Parameter("arg")]
            [Optional]
            [AllowEmpty]
            public TestEnum Enum_Optional { get; set; }
        }
    }
}