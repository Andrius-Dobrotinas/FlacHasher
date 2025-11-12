using NUnit.Framework; 
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Reflection; 

namespace Andy.Cmd.Parameter.ParameterValueResolver_Tests 
{ 
    public class AllowWhitespaceAttribute_Tests 
    { 
        ParameterValueResolver target = new ParameterValueResolver(); 

        [TestCase(nameof(TestParamsString.RegularWhitespaceAllowed), " ")] 
        [TestCase(nameof(TestParamsString.RegularWhitespaceAllowed), "\t")] 
        [TestCase(nameof(TestParamsString.OptionalWhitespaceAllowed), " ")] 
        [TestCase(nameof(TestParamsString.OptionalWhitespaceAllowed), "\t")] 
        [TestCase(nameof(TestParamsString.OptionalDefaultValue_WhitespaceAllowed), " ")] 
        [TestCase(nameof(TestParamsString.OptionalDefaultValue_WhitespaceAllowed), "\t")] 
        public void String__ValueIs_Whitespace__Must_Return_TheWhitespaceString(string propertyName, string value) 
        { 
            var argvs = new Dictionary<string, string[]> 
            { 
                { "arg", new string[] { value } } 
            }; 
            var result = new TestParamsString(); 
            var prop = typeof(TestParamsString).GetProperties().First(x => x.Name == propertyName); 
            target.ReadParameter(prop, argvs, result); 

            Assert.AreEqual(value, prop.GetValue(result)); 
        }

        [TestCase(nameof(TestParamsString.RegularWhitespaceAllowed), " asd ", "asd")] 
        [TestCase(nameof(TestParamsString.RegularWhitespaceAllowed), " a\tbdef\t", "a\tbdef")] 
        public void String__ValueIs_NonWhitespace__Must_Return_Trimmed(string propertyName, string value, string expectedValue) 
        { 
            var argvs = new Dictionary<string, string[]> 
            { 
                { "arg", new string[] { value } } 
            }; 
            var result = new TestParamsString(); 
            var prop = typeof(TestParamsString).GetProperties().First(x => x.Name == propertyName); 
            target.ReadParameter(prop, argvs, result); 

            Assert.AreEqual(expectedValue, prop.GetValue(result)); 
        }

        [TestCase(nameof(TestParamsString.RegularWhitespaceAllowed))] 
        [TestCase(nameof(TestParamsString.OptionalWhitespaceAllowed))] 
        [TestCase(nameof(TestParamsString.OptionalDefaultValue_WhitespaceAllowed))] 
        public void String__ValueIs_EmptyString__Must_ThrowException(string propertyName) 
        { 
            var argvs = new Dictionary<string, string[]> 
            { 
                { "arg", new string[] { "" } } 
            }; 
            var result = new TestParamsString(); 
            var prop = typeof(TestParamsString).GetProperties().First(x => x.Name == propertyName); 
            Assert.Throws<ParameterEmptyException>(() => target.ReadParameter(prop, argvs, result)); 
        } 

        [TestCase(nameof(TestParamsString.RegularWhitespaceAllowed))] 
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
        
        class TestParams_Array 
        { 
            [Parameter("arg")] 
            [AllowWhitespace] 
            public string[] WhitespaceAllowed { get; set; } 

            [Parameter("arg")] 
            [Optional] 
            [AllowWhitespace] 
            public string[] OptionalWhitespaceAllowed { get; set; } 
        } 

        class TestParamsString 
        { 
            [Parameter("arg")] 
            [AllowWhitespace] 
            public string RegularWhitespaceAllowed { get; set; } 

            [Parameter("arg")] 
            [Optional] 
            [AllowWhitespace] 
            public string OptionalWhitespaceAllowed { get; set; } 

            [Parameter("arg")] 
            [Optional(defaultValue: "x")] 
            [AllowWhitespace] 
            public string OptionalDefaultValue_WhitespaceAllowed { get; set; } 
        } 
    } 
}