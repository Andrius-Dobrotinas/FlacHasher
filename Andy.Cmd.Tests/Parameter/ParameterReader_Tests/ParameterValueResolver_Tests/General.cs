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
        [TestCase(nameof(TestParams.StringAnother), "Other Value", "Other Value")]
        [TestCase(nameof(TestParams.String_Optional), "arg 1 Optional value", "arg 1 Optional value")]
        [TestCase(nameof(TestParams.Primitive), "0", 0)]
        [TestCase(nameof(TestParams.Primitive), "10", 10)]
        [TestCase(nameof(TestParams.Primitive), "-5", -5)]
        [TestCase(nameof(TestParams.Primitive_Optional), "-1", -1)]
        [TestCase(nameof(TestParams.Primitive_Optional), "0", 0)]
        [TestCase(nameof(TestParams.Primitive_Bool), "true", true)]
        [TestCase(nameof(TestParams.Primitive_Bool), "false", false)]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "true", true)]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "false", false)]
        [TestCase(nameof(TestParams.Nullable_Primitive), "2", 2)]
        [TestCase(nameof(TestParams.Nullable_Primitive), "0", 0)]
        [TestCase(nameof(TestParams.Nullable_Primitive), "-1", -1)]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "true", true)]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "false", false)]
        [TestCase(nameof(TestParams.String_Optional_DefaultValue), "GT86", "GT86")]
        [TestCase(nameof(TestParams.Primitive_Optional_DefaultValue), "86", 86)]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional_DefaultValue_True), "false", false)]
        [TestCase(nameof(TestParams.Nullable_Primitive_Optional_DefaultValue), "86", 86)]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool_Optional_DefaultValue_True), "false", false)]
        [TestCase(nameof(TestParams.StringArray), "false", new[] { "false" })]
        [TestCase(nameof(TestParams.StringArray_Optional), "false", new[] { "false" })]
        public void Parse_ParameterValue__As_Its_Property_Type(string propertyName, string value, object expectedValue)
        {
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "wrong arg value" } },
                { attr.Name, new [] { value } },
                { "arg0", new [] { "wrong arg alue, again!" } }
            };

            var result = new TestParams();
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams2.String), new[] { "one", "three" }, "three")]
        [TestCase(nameof(TestParams2.String), new[] { "ichi", "ni", "go", "roku" }, "roku")]
        [TestCase(nameof(TestParams2.String_Optional), new[] { "ni", "ichi" }, "ichi")]
        [TestCase(nameof(TestParams2.Primitive), new[] { "1", "10" }, 10)]
        [TestCase(nameof(TestParams2.Primitive), new[] { "2", "1", "0" }, 0)]
        [TestCase(nameof(TestParams2.Primitive_Optional), new[] { "1", "7", "4" }, 4)]
        [TestCase(nameof(TestParams2.Nullable_Primitive), new[] { "12", "11" }, 11)]
        [TestCase(nameof(TestParams2.Nullable_Primitive), new[] { "2", "1", "0" }, 0)]
        [TestCase(nameof(TestParams2.Primitive_Bool), new[] { "false", "true" }, true)]
        [TestCase(nameof(TestParams2.Primitive_Bool), new[] { "true", "true", "false" }, false)]
        [TestCase(nameof(TestParams2.Primitive_Bool_Optional), new[] { "true", "true", "false" }, false)]
        [TestCase(nameof(TestParams2.Nullable_Primitive_Bool), new[] { "false", "true" }, true)]
        [TestCase(nameof(TestParams2.Nullable_Primitive_Bool), new[] { "true", "true", "false" }, false)]
        public void ManyValues_ProvidedFor_NonArray_Parameter__Must_Take_Last_Value(string propertyName, string[] input, object expected)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", input }
            };
            var result = new TestParams2();
            var prop = typeof(TestParams2).GetProperties().First(x => x.Name == propertyName);
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expected, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.String))]
        [TestCase(nameof(TestParams.StringAnother))]
        [TestCase(nameof(TestParams.Primitive))]
        [TestCase(nameof(TestParams.Primitive_Bool))]
        [TestCase(nameof(TestParams.StringArray))]
        [TestCase(nameof(TestParams.String_AllowEmpty))]
        [TestCase(nameof(TestParams.StringArray_AllowEmpty))]
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
        [TestCase(nameof(TestParams.StringArray))]
        [TestCase(nameof(TestParams.StringArray_Optional))]
        public void NonBoolean_Parameter_HasNoValue__Must_Reject__Regardless_Of_Optionality(string propertyName)
        {
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "arg 1 value" } },
                { attr.Name, new string[] { null } },
                { "arg0", new [] { "Other Value" } }
            };

            var result = new TestParams();

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
        [TestCase(nameof(TestParams.Primitive_Optional), "")]
        [TestCase(nameof(TestParams.Primitive_Optional), " ")]
        [TestCase(nameof(TestParams.Primitive_Optional), "\t")]
        [TestCase(nameof(TestParams.Primitive_Bool), "")]
        [TestCase(nameof(TestParams.Primitive_Bool), " ")]
        [TestCase(nameof(TestParams.Primitive_Bool), "\t")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), " ")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "\t")]
        [TestCase(nameof(TestParams.Nullable_Primitive), "")]
        [TestCase(nameof(TestParams.Nullable_Primitive), " ")]
        [TestCase(nameof(TestParams.Nullable_Primitive), "\t")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), " ")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "\t")]
        [TestCase(nameof(TestParams.StringArray), "")]
        [TestCase(nameof(TestParams.StringArray), " ")]
        [TestCase(nameof(TestParams.StringArray), "\t")]
        [TestCase(nameof(TestParams.StringArray_Optional), "")]
        [TestCase(nameof(TestParams.StringArray_Optional), " ")]
        [TestCase(nameof(TestParams.StringArray_Optional), "\t")]
        [TestCase(nameof(TestParams.StringArray_Optional_DefaultValue), "")]
        [TestCase(nameof(TestParams.StringArray_Optional_DefaultValue), " ")]
        [TestCase(nameof(TestParams.StringArray_Optional_DefaultValue), "\t")]
        public void EmptyString_OrWhitespace_Value__Must_Reject(string propertyName, string value)
        {
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var argvs = new Dictionary<string, string[]>()
            {
                { "--arg0", new [] { "arg 1 value" } },
                { attr.Name, new [] { value } },
                { "arg0", new [] { "Other Value" } }
            };

            var result = new TestParams();

            Assert.Throws<ParameterEmptyException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams.Primitive), "-")]
        [TestCase(nameof(TestParams.Primitive), "asd")]
        [TestCase(nameof(TestParams.Primitive), "10.6")]
        [TestCase(nameof(TestParams.Nullable_Primitive), "-")]
        [TestCase(nameof(TestParams.Nullable_Primitive), "asd")]
        [TestCase(nameof(TestParams.Nullable_Primitive), "10.6")]
        [TestCase(nameof(TestParams.Primitive_Optional), "-")]
        [TestCase(nameof(TestParams.Primitive_Optional), "asd")]
        [TestCase(nameof(TestParams.Primitive_Optional), "10.6")]
        [TestCase(nameof(TestParams.Primitive_Bool), "-")]
        [TestCase(nameof(TestParams.Primitive_Bool), "asd")]
        [TestCase(nameof(TestParams.Primitive_Bool), "-1")]
        [TestCase(nameof(TestParams.Primitive_Bool), "2")]
        [TestCase(nameof(TestParams.Primitive_Bool), "0.1")]
        [TestCase(nameof(TestParams.Primitive_Bool), "10")]
        [TestCase(nameof(TestParams.Primitive_Bool), "12.6")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "-")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "asd")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "-1")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "2")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "0.1")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "10")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), "12.6")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "-")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "asd")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "-1")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "2")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "0.1")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "10")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), "12.6")]
        [TestCase(nameof(TestParams.Primitive_Optional_DefaultValue), "GT86")]
        [TestCase(nameof(TestParams.Primitive_Bool_Optional_DefaultValue_True), "86")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Optional_DefaultValue), "GR86")]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool_Optional_DefaultValue_True), "86")]
        public void When__ParameterValue_Is_OfWrongType__Must_Reject(string propertyName, string rawValue)
        {
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var argvs = new Dictionary<string, string[]>()
            {
                { attr.Name, new [] { rawValue } }
            };
            var result = new TestParams();

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
        [TestCase(nameof(TestParamsMultiple.Primitive), "10", "8", "11", 10)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool), "true", "true", "false", true)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool), "false", "true", "true", false)]
        [TestCase(nameof(TestParamsMultiple.String_Optional), "value optional", "opt", "z", "value optional")]
        [TestCase(nameof(TestParamsMultiple.Primitive_Optional), "23", "66", "87", 23)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool_Optional), "true", "false", "false", true)]
        [TestCase(nameof(TestParamsMultiple.Nullable_Primitive), "66", "77", "1", 66)]
        [TestCase(nameof(TestParamsMultiple.Nullable_Primitive_Bool), "false", "true", "true", false)]
        [TestCase(nameof(TestParamsMultiple.StringArray), "word", "x", "nothing", new[] { "word" })]
        [TestCase(nameof(TestParamsMultiple.StringArray_Optional), "word", "x", "nothing", new[] { "word" })]
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
        [TestCase(nameof(TestParamsMultiple.Primitive), "10", 10)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool), "true", true)]
        [TestCase(nameof(TestParamsMultiple.String_Optional), "value optional", "value optional")]
        [TestCase(nameof(TestParamsMultiple.Primitive_Optional), "23", 23)]
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool_Optional), "true", true)]
        [TestCase(nameof(TestParamsMultiple.Nullable_Primitive), "76", 76)]
        [TestCase(nameof(TestParamsMultiple.Nullable_Primitive_Bool), "true", true)]
        [TestCase(nameof(TestParamsMultiple.StringArray), "nothing", new[] { "nothing" })]
        [TestCase(nameof(TestParamsMultiple.StringArray_Optional), "anything", new[] { "anything" })]
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
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool))]
        [TestCase(nameof(TestParamsMultiple.StringArray))]
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
        [TestCase(nameof(TestParamsMultiple.Primitive_Bool_Optional), false)]
        [TestCase(nameof(TestParamsMultiple.Nullable_Primitive), null)]
        [TestCase(nameof(TestParamsMultiple.Nullable_Primitive_Bool), null)]
        [TestCase(nameof(TestParamsMultiple.StringArray_Optional), null)]
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
        [TestCase(nameof(TestParams.Primitive_Bool_Optional), false)]
        [TestCase(nameof(TestParams.Nullable_Primitive), null)]
        [TestCase(nameof(TestParams.Nullable_Primitive_Bool), null)]
        [TestCase(nameof(TestParams.StringArray_Optional), null)]
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
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Bool_False), false)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Bool_True), true)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive), 555)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive_Bool_False), false)]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive_Bool_True), true)]
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
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Bool_False), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Bool_False), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Bool_True), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Primitive_Bool_True), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive_Bool_False), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive_Bool_False), "\t")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive_Bool_True), "")]
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive_Bool_True), "\t")]
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
        [TestCase(nameof(TestParams_OptionalWithDefaultValue.Nullable_Primitive))]
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
            [Parameter("--arg1")]
            public string String { get; set; }

            [Parameter("arg1")]
            public string StringAnother { get; set; }

            [Parameter("--arg-primitive")]
            public int Primitive { get; set; }

            [Parameter("--arg-bool")]
            public bool Primitive_Bool { get; set; }

            [Parameter("--arg-null-primitive")]
            public int? Nullable_Primitive { get; set; }

            [Parameter("--arg-null-bool")]
            public bool? Nullable_Primitive_Bool { get; set; }

            [Parameter("--arg1-optional")]
            [Optional]
            public string String_Optional { get; set; }

            [Parameter("--arg-primitive-optional")]
            [Optional]
            public int Primitive_Optional { get; set; }

            [Parameter("--arg-bool-optional")]
            [Optional]
            public bool Primitive_Bool_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: "wazaa!!")]
            public string String_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 667)]
            public int Primitive_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool Primitive_Bool_Optional_DefaultValue_True { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 555)]
            public int? Nullable_Primitive_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool? Nullable_Primitive_Bool_Optional_DefaultValue_True { get; set; }

            [Parameter("arg")]
            public string[] StringArray { get; set; }


            [Parameter("--arg1")]
            [AllowEmpty]
            public string String_AllowEmpty { get; set; }

            [Parameter("arg")]
            [Optional]
            public string[] StringArray_Optional { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: new string[] { "one" })]
            public string[] StringArray_Optional_DefaultValue { get; set; }

            [Parameter("arg")]
            [AllowEmpty]
            public string[] StringArray_AllowEmpty { get; set; }
        }

        class TestParams2
        {
            [Parameter("arg")]
            public string String { get; set; }

            [Parameter("arg")]
            public int Primitive { get; set; }

            [Parameter("arg")]
            public bool Primitive_Bool { get; set; }

            [Parameter("arg")]
            public int? Nullable_Primitive { get; set; }

            [Parameter("arg")]
            public bool? Nullable_Primitive_Bool { get; set; }

            [Parameter("arg")]
            [Optional]
            public string String_Optional { get; set; }

            [Parameter("arg")]
            [Optional]
            public int Primitive_Optional { get; set; }

            [Parameter("arg")]
            [Optional]
            public bool Primitive_Bool_Optional { get; set; }
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
            public bool Primitive_Bool_False { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool Primitive_Bool_True { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: 555)]
            public int? Nullable_Primitive { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: false)]
            public bool? Nullable_Primitive_Bool_False { get; set; }

            [Parameter("arg")]
            [Optional(defaultValue: true)]
            public bool? Nullable_Primitive_Bool_True { get; set; }

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
            public bool Primitive_Bool { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public int? Nullable_Primitive { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public bool? Nullable_Primitive_Bool { get; set; }

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
            public bool Primitive_Bool_Optional { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            public string[] StringArray { get; set; }

            [Parameter("--arg1", Order = 0)]
            [Parameter("Arg1", Order = 1)]
            [Parameter("Three", Order = 3)]
            [Optional]
            public string[] StringArray_Optional { get; set; }
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
    }
}