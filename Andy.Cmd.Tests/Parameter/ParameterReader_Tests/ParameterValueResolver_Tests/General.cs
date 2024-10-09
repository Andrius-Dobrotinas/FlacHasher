using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter.ParameterReader_Tests.ParameterValueResolver_Tests
{
    public class General
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase(nameof(TestParams_DiffererentParamNames.One), "arg 1 value", "arg 1 value")]
        [TestCase(nameof(TestParams_DiffererentParamNames.Two), "Other Value", "Other Value")]
        [TestCase(nameof(TestParams_DiffererentParamNames.Three), "arg 1 Another value", "arg 1 Another value")]
        public void PropertyValue_BasedOn_ParameterName_From_ParameterAttribute(string propertyName, string value, object expectedValue)
        {
            var prop = typeof(TestParams_DiffererentParamNames).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "wrong arg value" } },
                { attr.Name, new [] { value } },
                { "arg0", new [] { "wrong arg alue, again!" } }
            };

            var result = new TestParams_DiffererentParamNames();
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.String), "arg 1 value", "arg 1 value")]
        [TestCase(nameof(TestParams.String_Optional), "arg 1 Optional value", "arg 1 Optional value")]
        [TestCase(nameof(TestParams.String_Optional_DefaultValue), "GT86", "GT86")]
        [TestCase(nameof(TestParams.Primitive), "0", 0)]
        [TestCase(nameof(TestParams.Primitive), "10", 10)]
        [TestCase(nameof(TestParams.Primitive), "-5", -5)]
        [TestCase(nameof(TestParams.Primitive_Optional), "-1", -1)]
        [TestCase(nameof(TestParams.Primitive_Optional), "0", 0)]
        [TestCase(nameof(TestParams.Primitive_Optional_DefaultValue), "86", 86)]
        [TestCase(nameof(TestParams.Primitive_Nullable), "2", 2)]
        [TestCase(nameof(TestParams.Primitive_Nullable), "0", 0)]
        [TestCase(nameof(TestParams.Primitive_Nullable), "-1", -1)]
        [TestCase(nameof(TestParams.Primitive_Nullable_DefaultValue), "86", 86)]
        [TestCase(nameof(TestParams.Bool), "true", true)]
        [TestCase(nameof(TestParams.Bool), "false", false)]
        [TestCase(nameof(TestParams.Bool_Optional), "true", true)]
        [TestCase(nameof(TestParams.Bool_Optional), "false", false)]
        [TestCase(nameof(TestParams.Bool_Optional_DefaultValue_True), "false", false)]
        [TestCase(nameof(TestParams.Bool_Nullable), "true", true)]
        [TestCase(nameof(TestParams.Bool_Nullable), "false", false)]
        [TestCase(nameof(TestParams.Bool_Nullable_DefaultValue_True), "false", false)]
        [TestCase(nameof(TestParams.Array), "false", new[] { "false" })]
        [TestCase(nameof(TestParams.Array_Optional), "false", new[] { "false" })]
        [TestCase(nameof(TestParams.Enum), "Two", TestEnum.Two, Description = "There are more tests for this a tests file dedicated to Enum type")]
        public void Parse_ParameterValue__As_Its_Property_Type(string propertyName, string value, object expectedValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "wrong arg value" } },
                { "arg", new [] { value } },
                { "arg0", new [] { "wrong arg alue, again!" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.String), new[] { "one", "three" }, "three")]
        [TestCase(nameof(TestParams.String), new[] { "ichi", "ni", "go", "roku" }, "roku")]
        [TestCase(nameof(TestParams.String_Optional), new[] { "ni", "ichi" }, "ichi")]
        [TestCase(nameof(TestParams.Primitive), new[] { "1", "10" }, 10)]
        [TestCase(nameof(TestParams.Primitive), new[] { "2", "1", "0" }, 0)]
        [TestCase(nameof(TestParams.Primitive_Optional), new[] { "1", "7", "4" }, 4)]
        [TestCase(nameof(TestParams.Primitive_Nullable), new[] { "12", "11" }, 11)]
        [TestCase(nameof(TestParams.Primitive_Nullable), new[] { "2", "1", "0" }, 0)]
        [TestCase(nameof(TestParams.Bool), new[] { "false", "true" }, true)]
        [TestCase(nameof(TestParams.Bool), new[] { "true", "true", "false" }, false)]
        [TestCase(nameof(TestParams.Bool_Optional), new[] { "true", "true", "false" }, false)]
        [TestCase(nameof(TestParams.Bool_Nullable), new[] { "false", "true" }, true)]
        [TestCase(nameof(TestParams.Bool_Nullable), new[] { "true", "true", "false" }, false)]
        [TestCase(nameof(TestParams.Enum), new[] { "Two", "One", "Three" }, TestEnum.Three)]
        [TestCase(nameof(TestParams.Enum_Optional), new[] { "Three", "One", "Two" }, TestEnum.Two )]
        [TestCase(nameof(TestParams.Enum_Nullable), new[] { "Two", "Two", "One" }, TestEnum.One)]
        public void ManyValues_ProvidedFor_NonArray_Parameter__Must_Take_Last_Value(string propertyName, string[] input, object expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", input }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.String))]
        [TestCase(nameof(TestParams.String_AllowEmpty))]
        [TestCase(nameof(TestParams.Primitive))]
        [TestCase(nameof(TestParams.Bool))]
        [TestCase(nameof(TestParams.Enum))]
        [TestCase(nameof(TestParams.Array))]
        [TestCase(nameof(TestParams.Array_AllowEmpty))]
        public void Mandatory_Parameter_NotProvided__Must_Reject__RegardlessOf_AllowEmpty_Attribute(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg-irrelevant", new [] { "value" } }
            };

            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);

            var exception = Assert.Throws<ParameterMissingException>(() => target.ReadParameter(prop, argvs, result));
            Assert.AreEqual(prop, exception.ParameterProperty);
        }

        [TestCase(nameof(TestParams.String))]
        [TestCase(nameof(TestParams.String_Optional))]
        [TestCase(nameof(TestParams.Primitive))]
        [TestCase(nameof(TestParams.Primitive_Optional))]
        [TestCase(nameof(TestParams.Enum))]
        [TestCase(nameof(TestParams.Enum_Optional))]
        [TestCase(nameof(TestParams.Array))]
        [TestCase(nameof(TestParams.Array_Optional))]
        public void NonBoolean_Parameter_HasNoValue__Must_Reject__Regardless_Of_Optionality(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "arg 1 value" } },
                { "arg", new string[] { null } },
                { "arg0", new [] { "Other Value" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<ParameterEmptyException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams.String), "")]
        [TestCase(nameof(TestParams.String), " ")]
        [TestCase(nameof(TestParams.String), "\t")]
        [TestCase(nameof(TestParams.String_Optional), "")]
        [TestCase(nameof(TestParams.String_Optional), " ")]
        [TestCase(nameof(TestParams.String_Optional), "\t")]
        [TestCase(nameof(TestParams.Primitive), "")]
        [TestCase(nameof(TestParams.Primitive), " ")]
        [TestCase(nameof(TestParams.Primitive), "\t")]
        [TestCase(nameof(TestParams.Primitive_Nullable), "")]
        [TestCase(nameof(TestParams.Primitive_Nullable), " ")]
        [TestCase(nameof(TestParams.Primitive_Nullable), "\t")]
        [TestCase(nameof(TestParams.Primitive_Optional), "")]
        [TestCase(nameof(TestParams.Primitive_Optional), " ")]
        [TestCase(nameof(TestParams.Primitive_Optional), "\t")]
        [TestCase(nameof(TestParams.Bool), "")]
        [TestCase(nameof(TestParams.Bool), " ")]
        [TestCase(nameof(TestParams.Bool), "\t")]
        [TestCase(nameof(TestParams.Bool_Nullable), "")]
        [TestCase(nameof(TestParams.Bool_Nullable), " ")]
        [TestCase(nameof(TestParams.Bool_Nullable), "\t")]
        [TestCase(nameof(TestParams.Bool_Optional), "")]
        [TestCase(nameof(TestParams.Bool_Optional), " ")]
        [TestCase(nameof(TestParams.Bool_Optional), "\t")]
        [TestCase(nameof(TestParams.Enum), "")]
        [TestCase(nameof(TestParams.Enum), " ")]
        [TestCase(nameof(TestParams.Enum), "\t")]
        [TestCase(nameof(TestParams.Enum_Optional), "")]
        [TestCase(nameof(TestParams.Enum_Optional), " ")]
        [TestCase(nameof(TestParams.Enum_Optional), "\t")]
        [TestCase(nameof(TestParams.Enum_Nullable), "")]
        [TestCase(nameof(TestParams.Enum_Nullable), " ")]
        [TestCase(nameof(TestParams.Enum_Nullable), "\t")]
        [TestCase(nameof(TestParams.Array), "")]
        [TestCase(nameof(TestParams.Array), " ")]
        [TestCase(nameof(TestParams.Array), "\t")]
        [TestCase(nameof(TestParams.Array_Optional), "")]
        [TestCase(nameof(TestParams.Array_Optional), " ")]
        [TestCase(nameof(TestParams.Array_Optional), "\t")]
        [TestCase(nameof(TestParams.Array_Optional_DefaultValue), "")]
        [TestCase(nameof(TestParams.Array_Optional_DefaultValue), " ")]
        [TestCase(nameof(TestParams.Array_Optional_DefaultValue), "\t")]
        public void EmptyString_OrWhitespace_Value__Must_Reject(string propertyName, string value)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "arg 1 value" } },
                { "arg", new [] { value } },
                { "arg0", new [] { "Other Value" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<ParameterEmptyException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams.Primitive), "-")]
        [TestCase(nameof(TestParams.Primitive), "asd")]
        [TestCase(nameof(TestParams.Primitive), "10.6")]
        [TestCase(nameof(TestParams.Primitive_Nullable), "-")]
        [TestCase(nameof(TestParams.Primitive_Nullable), "asd")]
        [TestCase(nameof(TestParams.Primitive_Nullable), "10.6")]
        [TestCase(nameof(TestParams.Primitive_Optional), "-")]
        [TestCase(nameof(TestParams.Primitive_Optional), "asd")]
        [TestCase(nameof(TestParams.Primitive_Optional), "10.6")]
        [TestCase(nameof(TestParams.Primitive_Optional_DefaultValue), "GT86")]
        [TestCase(nameof(TestParams.Primitive_Nullable_DefaultValue), "GR86")]
        [TestCase(nameof(TestParams.Bool), "-")]
        [TestCase(nameof(TestParams.Bool), "asd")]
        [TestCase(nameof(TestParams.Bool), "-1")]
        [TestCase(nameof(TestParams.Bool), "2")]
        [TestCase(nameof(TestParams.Bool), "0.1")]
        [TestCase(nameof(TestParams.Bool), "10")]
        [TestCase(nameof(TestParams.Bool), "12.6")]
        [TestCase(nameof(TestParams.Bool_Nullable), "-")]
        [TestCase(nameof(TestParams.Bool_Nullable), "asd")]
        [TestCase(nameof(TestParams.Bool_Nullable), "-1")]
        [TestCase(nameof(TestParams.Bool_Nullable), "2")]
        [TestCase(nameof(TestParams.Bool_Nullable), "0.1")]
        [TestCase(nameof(TestParams.Bool_Nullable), "10")]
        [TestCase(nameof(TestParams.Bool_Nullable), "12.6")]
        [TestCase(nameof(TestParams.Bool_Optional), "-")]
        [TestCase(nameof(TestParams.Bool_Optional), "asd")]
        [TestCase(nameof(TestParams.Bool_Optional), "-1")]
        [TestCase(nameof(TestParams.Bool_Optional), "2")]
        [TestCase(nameof(TestParams.Bool_Optional), "0.1")]
        [TestCase(nameof(TestParams.Bool_Optional), "10")]
        [TestCase(nameof(TestParams.Bool_Optional), "12.6")]
        [TestCase(nameof(TestParams.Bool_Optional_DefaultValue_True), "86")]
        [TestCase(nameof(TestParams.Bool_Nullable_DefaultValue_True), "86")]
        [TestCase(nameof(TestParams.Enum), "Whatevz", Description = "There are more tests for this a tests file dedicated to Enum type")]
        [TestCase(nameof(TestParams.Enum_Optional), "Whatevz")]
        [TestCase(nameof(TestParams.Enum_Optional_DefaultValue), "Whatevz")]
        public void When__ParameterValue_Is_OfWrongType__Must_Reject(string propertyName, string rawValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg", new [] { rawValue } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<BadParameterValueException>(() => target.ReadParameter(prop, argvs, result));
        }

        [Test]
        public void Inheritance__Must_Ignore_BaseProperty_ParameterConfiguration__When_OverridingProperty_HasNoParamAttr()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg1-base", new [] { "arg 1 value" } },
            };
            var result = new TestParamsInheritance1();
            var prop = typeof(TestParamsInheritance1).GetProperties().First(x => x.Name == nameof(TestParamsInheritance1.One));
            target.ReadParameter(prop, argvs, result);

            Assert.IsNull(result.One);
        }

        [Test]
        public void Inheritance__Must_Ignore_BaseProperty_ParameterConfiguration__When_OverridingProperty_HasParamAttr()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg1-base", new [] { "arg 1 base" } },
                { "--arg1-new", new [] { "arg 1 new" } },
            };
            var result = new TestParamsInheritance2();
            var prop = typeof(TestParamsInheritance2).GetProperties().First(x => x.Name == nameof(TestParamsInheritance2.One));
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual("arg 1 new", result.One);
        }

        [Test]
        public void ParameterLookup_ConvertNameToLowercase()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg3", new [] { "arg value" } }
            };
            var result = new TestParams_CaseSensitivity();
            var prop = typeof(TestParams_CaseSensitivity).GetProperties().First(x => x.Name == nameof(TestParams_CaseSensitivity.Three));
            target.ReadParameter(prop, argvs, result, inLowercase: true);

            Assert.AreEqual("arg value", result.Three);
        }

        [Test]
        public void ParameterLookup_PreserveNameLettercasing()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg3", new [] { "arg value" } }
            };
            var result = new TestParams_CaseSensitivity();
            var prop = typeof(TestParams_CaseSensitivity).GetProperties().First(x => x.Name == nameof(TestParams_CaseSensitivity.Three));

            Assert.Throws<ParameterMissingException>(
                () => target.ReadParameter(prop, argvs, result, inLowercase: false));
        }

        [TestCase(nameof(TestParamsMultiple.String), "value 1", "value 2", "value 3", "value 1")]
        [TestCase(nameof(TestParamsMultiple.String), "x", "value too", "another", "x")]
        [TestCase(nameof(TestParamsMultiple.String_Optional), "value optional", "opt", "z", "value optional")]
        [TestCase(nameof(TestParamsMultiple.Primitive), "10", "8", "11", 10)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Nullable), "66", "77", "1", 66)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Optional), "23", "66", "87", 23)]
        [TestCase(nameof(TestParamsMultiple.Bool), "true", "true", "false", true)]
        [TestCase(nameof(TestParamsMultiple.Bool), "false", "true", "true", false)]
        [TestCase(nameof(TestParamsMultiple.Bool_Nullable), "false", "true", "true", false)]
        [TestCase(nameof(TestParamsMultiple.Bool_Optional), "true", "false", "false", true)]
        [TestCase(nameof(TestParamsMultiple.Enum), "Two", "One", "Three", TestEnum.Two)]
        [TestCase(nameof(TestParamsMultiple.Enum_Optional), "Two", "One", "Three", TestEnum.Two)]
        [TestCase(nameof(TestParamsMultiple.Enum_Nullable), "Two", "One", "Three", TestEnum.Two)]
        [TestCase(nameof(TestParamsMultiple.Array), "word", "x", "nothing", new[] { "word" })]
        [TestCase(nameof(TestParamsMultiple.Array_Optional), "word", "x", "nothing", new[] { "word" })]
        public void Multiple_ParamterSources_Specified_And_AllArePresent__Must_TakeOne_WithLowestOrder(string propertyName, string firstValue, string secondValue, string thirdValue, object expectedValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "Arg1", new [] { secondValue } },
                { "--arg1", new [] { firstValue } },
                { "Three", new [] { thirdValue } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        // TODO: I'm pretty sure Optional+DefaultValue won't pass this -- it will use the default value for the mising param and call it a day
        [TestCase(nameof(TestParamsMultiple.String), "value 1", "value 1")]
        [TestCase(nameof(TestParamsMultiple.String), "value too", "value too")]
        [TestCase(nameof(TestParamsMultiple.String_Optional), "value optional", "value optional")]
        [TestCase(nameof(TestParamsMultiple.Primitive), "10", 10)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Optional), "23", 23)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Nullable), "76", 76)]
        [TestCase(nameof(TestParamsMultiple.Bool), "true", true)]
        [TestCase(nameof(TestParamsMultiple.Bool_Optional), "true", true)]
        [TestCase(nameof(TestParamsMultiple.Bool_Nullable), "true", true)]
        [TestCase(nameof(TestParamsMultiple.Enum), "Three", TestEnum.Three)]
        [TestCase(nameof(TestParamsMultiple.Enum_Nullable), "Two", TestEnum.Two)]
        [TestCase(nameof(TestParamsMultiple.Enum_Optional), "Three", TestEnum.Three)]
        [TestCase(nameof(TestParamsMultiple.Array), "nothing", new[] { "nothing" })]
        [TestCase(nameof(TestParamsMultiple.Array_Optional), "anything", new[] { "anything" })]
        public void Multiple_ParamterSources_Specified_And_SomeAreNotPresent__Must_TakeOne_WithLowestOrder(string propertyName, string value, object expectedValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--somethinElse", new [] { "value" } },
                { "Arg1", new [] { value } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParamsMultiple.String))]
        [TestCase(nameof(TestParamsMultiple.Primitive))]
        [TestCase(nameof(TestParamsMultiple.Bool))]
        [TestCase(nameof(TestParamsMultiple.Enum))]
        [TestCase(nameof(TestParamsMultiple.Array))]
        public void Multiple_ParameterSources_Specified_And_NoneArePresent__IsMandatory__Must_Reject(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--somethinElse", new [] { "value" } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == propertyName);
            Assert.Throws<ParameterMissingException>(
                () => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParamsMultiple.String_Optional), null)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Optional), 0)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Nullable), null)]
        [TestCase(nameof(TestParamsMultiple.Bool_Optional), false)]
        [TestCase(nameof(TestParamsMultiple.Bool_Nullable), null)]
        [TestCase(nameof(TestParamsMultiple.Enum_Optional), default(TestEnum))]
        [TestCase(nameof(TestParamsMultiple.Enum_Nullable), null)]
        [TestCase(nameof(TestParamsMultiple.Array_Optional), null)]
        public void Multiple_ParameterSouces_Specified_And_NoneArePresent__IsOptional__Must_BeCool(string propertyName, object expectedValue)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "--somethinElse", new [] { "value" } }
            };

            var result = new TestParamsMultiple();
            var prop = typeof(TestParamsMultiple).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);
            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.String_Optional), null)]
        [TestCase(nameof(TestParams.Primitive_Optional), 0)]
        [TestCase(nameof(TestParams.Primitive_Nullable), null)]
        [TestCase(nameof(TestParams.Bool_Optional), false)]
        [TestCase(nameof(TestParams.Bool_Nullable), null)]
        [TestCase(nameof(TestParams.Enum_Optional), default(TestEnum))]
        [TestCase(nameof(TestParams.Enum_Nullable), null)]
        [TestCase(nameof(TestParams.Array_Optional), null)]
        public void Optional__Parameter_NotProvided__Must__Return_DefaultValue_ForTheType(string propertyName, object expectedValue)
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams_OptionalWithDefaultValue.String), "wazaa!!")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive), 667)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Nullable), 555)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_False), false)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_True), true)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_Nullable_False), false)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_Nullable_True), true)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum), TestEnum.Two)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum_Nullable), TestEnum.Three)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Array), new string[] { "wazaa!!" } )]
        public void Optional__With_DefaultValue_Specified__Parameter_NotProvided__Must__Return_Configured_DefaultValue(string propertyName, object expectedValue)
        {
            var argvs = new Dictionary<string, string[]>();
            var result = new TestParams_OptionalWithDefaultValue();
            var prop = typeof(TestParams_OptionalWithDefaultValue).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams_OptionalWithDefaultValue.String), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.String), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Nullable), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Nullable), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_False), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_False), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_True), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_True), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_Nullable_False), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_Nullable_False), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_Nullable_True), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Bool_Nullable_True), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum_Nullable), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum_Nullable), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Array), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Array), "\t")]
        public void Optional__With_DefaultValue_Specified__Parameter_ValueIs_EmptyString_OrWhitespace__Must_Reject(string propertyName, string value)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams_OptionalWithDefaultValue();
            var prop = typeof(TestParams_OptionalWithDefaultValue).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<ParameterEmptyException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams_OptionalWithDefaultValue.String))]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive))]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Nullable))]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Enum))]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Array))]
        public void Optional__With_DefaultValue_Specified__NonBoolean_Parameter_NoValue__Must_Reject(string propertyName)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "arg", new string[] { null } }
            };
            var result = new TestParams_OptionalWithDefaultValue();
            var prop = typeof(TestParams_OptionalWithDefaultValue).GetProperties().First(x => x.Name == propertyName);

            Assert.Throws<ParameterEmptyException>(() => target.ReadParameter(prop, argvs, result));
        }

        class TestParams_DiffererentParamNames
        {
            [Parameter("--arg1")]
            public string One { get; set; }

            [Parameter("arg1")]
            public string Two { get; set; }

            [Parameter("Third")]
            public string Three { get; set; }
        }

        class TestParams
        {
            [Parameter("arg")]
            public string String { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public string String_AllowEmpty { get; set; }

            [Parameter("arg")]
            [Optional]
            public string String_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: "wazaa!!")]
            public string String_Optional_DefaultValue { get; set; }


            [Parameter("arg")]
            public int Primitive { get; set; }

            [Parameter("arg")]
            [Optional]
            public int Primitive_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 667)]
            public int Primitive_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            public int? Primitive_Nullable { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 555)]
            public int? Primitive_Nullable_DefaultValue { get; set; }


            [Parameter("arg")]
            public bool Bool { get; set; }

            [Parameter("arg")]
            [Optional]
            public bool Bool_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool Bool_Optional_DefaultValue_True { get; set; }

            [Parameter("arg")]
            public bool? Bool_Nullable { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool? Bool_Nullable_DefaultValue_True { get; set; }


            [Parameter("arg")]
            public TestEnum Enum { get; set; }

            [Parameter("arg")]
            [Optional]
            public TestEnum Enum_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: TestEnum.Two)]
            public TestEnum Enum_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            public TestEnum? Enum_Nullable { get; set; }


            [Parameter("arg")]
            public string[] Array { get; set; }

            [Parameter("arg")]
            [Optional]
            public string[] Array_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: new string[] { "one" })]
            public string[] Array_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public string[] Array_AllowEmpty { get; set; }
        }

        class TestParams_CaseSensitivity
        {
            [Parameter("ArG3")]
            public string Three { get; set; }
        }

        class TestParams_OptionalWithDefaultValue
        {
            [Parameter("arg")]
            [Optional(defaultValue: "wazaa!!")]
            public string String { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 667)]
            public int Primitive { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: false)]
            public bool Bool_False { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool Bool_True { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 555)]
            public int? Primitive_Nullable { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: false)]
            public bool? Bool_Nullable_False { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool? Bool_Nullable_True { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: TestEnum.Two)]
            public TestEnum Enum { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: TestEnum.Three)]
            public TestEnum Enum_Nullable { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: new string[] { "wazaa!!" })]
            public string[] Array { get; set; }
        }

        class TestParamsMultiple
        {
            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public string String { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public int Primitive { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public bool Bool { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public int? Primitive_Nullable { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public bool? Bool_Nullable { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            [Optional]
            public string String_Optional { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            [Optional]
            public int Primitive_Optional { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            [Optional]
            public bool Bool_Optional { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public string[] Array { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            [Optional]
            public string[] Array_Optional { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public TestEnum Enum { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public TestEnum? Enum_Nullable { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            [Optional]
            public TestEnum Enum_Optional { get; set; }
        }

        class TestParamsBase
        {
            [Parameter("--arg1-base")]
            public virtual string One { get; set; }
        }

        class TestParamsInheritance1 : TestParamsBase
        {
            public override string One { get; set; }
        }

        class TestParamsInheritance2 : TestParamsBase
        {
            [Parameter("--arg1-new")]
            public override string One { get; set; }
        }

        public enum TestEnum
        {
            One = 0,
            Two = 1,
            Three = 2,
        }
    }
}