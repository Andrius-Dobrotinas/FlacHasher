using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd
{
    public class ArgumentSplitter_Tests
    {
        [TestCase("arg1=valu1", "arg1", "valu1")]
        [TestCase("Arg1=Value with spaces", "Arg1", "Value with spaces")]
        [TestCase("Argument=Value \"with quotes\"", "Argument", "Value \"with quotes\"")]
        [TestCase("Argument=\"Value with quotes\"", "Argument", "\"Value with quotes\"")]
        [TestCase("arg1=", "arg1", "")]
        public void Split_EachArguments_Into_KeyValue_At_EqualsSign(string input, string expectedArgName, string expectedValue)
        {
            var result = ArgumentSplitter.GetArguments(new string[] { input });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expectedArgName, result.Keys.First());
            var value = result.Values.First();
            Assert.AreEqual(1, value.Length, "Value array has to contain a single element");
            Assert.AreEqual(expectedValue, value.First());
        }

        [TestCase("arg1")]
        [TestCase("Argument")]
        public void When_ArgumentIsProvided_Without_EqualsSign__Must_Put_Null_Value(string input)
        {
            var result = ArgumentSplitter.GetArguments(new string[] { input });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(input, result.Keys.First());
            var value = result.Values.First();
            Assert.NotNull(value);
            Assert.AreEqual(1, value.Length, "Value array has to contain a single element");
            Assert.AreEqual(null, value.First());
        }

        [TestCase("arg1=Valu1", "arg1", "Valu1")]
        [TestCase("ARG=Valu1", "arg", "Valu1")]
        [TestCase("aRGument=Valu1", "argument", "Valu1")]
        public void Convert_ArgumentNames_ToLowercase_When_Requested(string input, string expectedArgName, string expectedValue)
        {
            var result = ArgumentSplitter.GetArguments(new string[] { input }, paramNamesToLowercase: true);

            Assert.AreEqual(expectedArgName, result.Keys.First());
            Assert.AreEqual(expectedValue, result.Values.First().First());
        }

        [TestCase(new[] { "arg1=Valu1", "arg1=Value2" }, 1, "arg1", new[] { "Valu1", "Value2" })]
        [TestCase(new[] { "arg1=Value2", "arg1=Valu1" }, 1, "arg1", new[] { "Value2", "Valu1" })]
        [TestCase(new[] { "arg1=Valu1", "arg1=", "arg1=Value2", "arg1=" }, 1, "arg1", new[] { "Valu1", "", "Value2", "" })]
        [TestCase(new[] { "arg1=Valu1", "arg2=", "arg1=Value2", "arg3=somethin' else" }, 3, "arg1", new[] { "Valu1", "Value2" })]
        public void RepeatedArgument__Values_Must_BeGrouped_Under_One_Key__PreservingTheOriginalOrder(string[] input, int expectedArgCount, string expectedArgName, string[] expectedValue)
        {
            var result = ArgumentSplitter.GetArguments(input);

            Assert.AreEqual(expectedArgCount, result.Count);

            Assert.Contains(expectedArgName, result.Keys.ToArray());
            Assert.AreEqual(expectedValue, result[expectedArgName]);
        }

        [TestCase("arg1=Valu1", "Arg1=Value2")]
        public void ArgumentGrouping_HasToBe_CaseSensitive(params string[] input)
        {
            var result = ArgumentSplitter.GetArguments(input);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result.First().Value.Length);
            Assert.AreEqual(1, result.Skip(1).First().Value.Length);
        }

        [TestCase("arg1=Valu1", "Arg1=Value2")]
        public void ArgumentGrouping_HasToBe_CaseSensitive__When_Requested(params string[] input)
        {
            var result = ArgumentSplitter.GetArguments(input, paramNamesToLowercase: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.First().Value.Length);
        }

        [TestCase(new[] { "arg1=Valu1", "arg1=Val Kilmer", "arg1" }, new[] { "Valu1", "Val Kilmer" })]
        [TestCase(new[] { "arg1", "arg1", "arg1" }, new string[] { null })]
        [TestCase(new[] { "arg1=Valu1", "arg1", "arg1=" }, new[] { "Valu1", "" })]
        public void RepeatedArgument__Must_Discard_Repeated_Args_Without_Values(string[] input, string[] expectedValue)
        {
            var result = ArgumentSplitter.GetArguments(input);

            Assert.AreEqual(1, result.Count);
            var values = result.Values.First();
            AssertThat.CollectionsMatchExactly(values, expectedValue);
            Assert.AreEqual(expectedValue, values);
        }
    }
}