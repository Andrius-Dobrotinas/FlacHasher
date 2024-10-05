using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Andy.Cmd.Parameter.ParameterReader_Tests.ParameterValueResolver_Tests.TypeSpecific
{
    public class ArrayOfStringType
    {
        ParameterValueResolver target = new ParameterValueResolver();

        [TestCase(nameof(TestParams.Regular), "0;One;three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Regular), "2", new[] { "2" })]
        [TestCase(nameof(TestParams.Regular), "First;Se Cond", new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.Optional), "0;One;three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Optional), "2", new[] { "2" })]
        [TestCase(nameof(TestParams.Optional), "First;Se Cond", new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.EmptyAllowed), "0;One;three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.EmptyAllowed), "2", new[] { "2" })]
        [TestCase(nameof(TestParams.EmptyAllowed), "First;Se Cond", new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "0;One;three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "2", new[] { "2" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "First;Se Cond", new[] { "First", "Se Cond" })]
        public void Value_ProvidedAs_SingleArgument__Must_ParseIt_As_SemicolonSeparated(string propertyName, string value, string[] expectedValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var result = new TestParams();
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.Regular), new[] { "0", "One", "three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Regular), new[] { "2" }, new[] { "2" })]
        [TestCase(nameof(TestParams.Regular), new[] { "First", "Se Cond" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.Regular), new[] { "First;Second", "Se;Cond" }, new[] { "First;Second", "Se;Cond" }, Description = "Even when there are separators within values")]
        [TestCase(nameof(TestParams.Optional), new[] { "0", "One", "three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Optional), new[] { "2" }, new[] { "2" })]
        [TestCase(nameof(TestParams.Optional), new[] { "First", "Se Cond" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.Optional), new[] { "First;Second", "Se;Cond" }, new[] { "First;Second", "Se;Cond" }, Description = "Even when there are separators within values")]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { "0", "One", "three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { "2" }, new[] { "2" })]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { "First", "Se Cond" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { "First;Second", "Se;Cond" }, new[] { "First;Second", "Se;Cond" }, Description = "Even when there are separators within values")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { "0", "One", "three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { "2" }, new[] { "2" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { "First", "Se Cond" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { "First;Second", "Se;Cond" }, new[] { "First;Second", "Se;Cond" }, Description = "Even when there are separators within values")]
        public void Value_ProvidedAs_DiscreteArguments__Must_TreatThem_As_Discrete_ArrayItems(string propertyName, string[] values, string[] expectedValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", values }
            };
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var result = new TestParams();
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.Regular), " 0\t", new[] { "0" })]
        [TestCase(nameof(TestParams.Regular), " 0;One   ; three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Regular), "\tFirst;   Se Cond ", new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.Optional), " 0\t", new[] { "0" })]
        [TestCase(nameof(TestParams.Optional), " 0;One   ; three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Optional), "\tFirst;   Se Cond ", new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.EmptyAllowed), " 0\t", new[] { "0" })]
        [TestCase(nameof(TestParams.EmptyAllowed), " 0;One   ; three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.EmptyAllowed), "\tFirst;   Se Cond ", new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), " 0\t", new[] { "0" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), " 0;One   ; three", new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "\tFirst;   Se Cond ", new[] { "First", "Se Cond" })]
        public void Trim_Values__When_ProvidedAs_SingleArgument(string propertyName, string value, string[] expectedValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var result = new TestParams();
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.Regular), new[] { " 0\t" }, new[] { "0" })]
        [TestCase(nameof(TestParams.Regular), new[] { "0    ", "One ", " three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Regular), new[] { "First ", "Se Cond\t" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.Optional), new[] { " 0\t" }, new[] { "0" })]
        [TestCase(nameof(TestParams.Optional), new[] { "0    ", "One ", " three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.Optional), new[] { "First ", "Se Cond\t" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { " 0\t" }, new[] { "0" })]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { "0    ", "One ", " three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.EmptyAllowed), new[] { "First ", "Se Cond\t" }, new[] { "First", "Se Cond" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { " 0\t" }, new[] { "0" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { "0    ", "One ", " three" }, new[] { "0", "One", "three" })]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), new[] { "First ", "Se Cond\t" }, new[] { "First", "Se Cond" })]
        public void Trim_Values__When_ProvidedAs_DiscreteArguments(string propertyName, string[] value, string[] expectedValue)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", value }
            };
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            var attr = prop.GetCustomAttribute<ParameterAttribute>();

            var result = new TestParams();
            target.ReadParameter(prop, argvs, result);

            Assert.AreEqual(expectedValue, prop.GetValue(result));
        }

        [TestCase(nameof(TestParams.Regular), ";")]
        [TestCase(nameof(TestParams.Regular), ";;")]
        [TestCase(nameof(TestParams.Regular), "; ")]
        [TestCase(nameof(TestParams.Regular), "; ;")]
        [TestCase(nameof(TestParams.Optional), ";;")]
        [TestCase(nameof(TestParams.EmptyAllowed), ";;")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), ";;")]
        public void EmptyElements__Value_ProvidedAs_SingleArgument__IsSeparatorsOnly__Must_Reject__RegardlessOf_AllowEmpty_Attribute(string propertyName, string value)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { value } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            Assert.Throws<BadParameterValueException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams.Regular), "", "good")]
        [TestCase(nameof(TestParams.Regular), "good", "")]
        [TestCase(nameof(TestParams.Regular), " ", "great")]
        [TestCase(nameof(TestParams.Regular), "great", " ")]
        [TestCase(nameof(TestParams.Regular), "\t", "not bad")]
        [TestCase(nameof(TestParams.Regular), "not bad", "\t")]
        [TestCase(nameof(TestParams.Regular), "a'ight?", null)]
        [TestCase(nameof(TestParams.Regular), null, "a'ight")]
        [TestCase(nameof(TestParams.Optional), "", "good")]
        [TestCase(nameof(TestParams.Optional), "good", "\t")]
        [TestCase(nameof(TestParams.EmptyAllowed), "", "good")]
        [TestCase(nameof(TestParams.EmptyAllowed), "good", "\t")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "", "good")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "good", "\t")]
        public void EmptyElements__Value_ProvidedAs_DiscreteArguments_Contains_EmptyElements__Must_Reject__RegardlessOf_AllowEmpty_Attribute(string propertyName, params string[] values)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", values }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            Assert.Throws<BadParameterValueException>(() => target.ReadParameter(prop, argvs, result));
        }

        [TestCase(nameof(TestParams.Regular), ";good")]
        [TestCase(nameof(TestParams.Regular), "good;")]
        [TestCase(nameof(TestParams.Regular), " ;great")]
        [TestCase(nameof(TestParams.Regular), "\t;not bad")]
        [TestCase(nameof(TestParams.Optional), ";good")]
        [TestCase(nameof(TestParams.Optional), "good;\t")]
        [TestCase(nameof(TestParams.EmptyAllowed), ";good")]
        [TestCase(nameof(TestParams.EmptyAllowed), "good;\t")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), ";good")]
        [TestCase(nameof(TestParams.OptionalEmptyAllowed), "good;\t")]
        public void EmptyElements__Value_ProvidedAs_SingleArgument_Contains_EmptyElements__Must_Reject__RegardlessOf_AllowEmpty_Attribute(string propertyName, params string[] values)
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", values }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == propertyName);
            Assert.Throws<BadParameterValueException>(() => target.ReadParameter(prop, argvs, result));
        }

        // TODO: do this on a higher level
        [Test]
        public void Optional_DefaulValue__NotSupported()
        {
            var argvs = new Dictionary<string, string[]>
            {
                { "arg", new [] { "value" } }
            };
            var result = new TestParams();
            var prop = typeof(TestParams).GetProperties().First(x => x.Name == nameof(TestParams.OptionalDefaultValue));

            Assert.Throws<NotSupportedException>(() => target.ReadParameter(prop, argvs, result));
        }

        class TestParams
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
    }
}