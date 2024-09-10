using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_Tests
    {
        [Test]
        public void When__MasterProperty_HasValue__And_Target_NoValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "good value" } }
            };

            var exception = Assert.Throws<ParameterMissingException>(() => ParameterReader.GetParameters<TestParams>(argvs));
            Assert.AreEqual("dependency", exception.ParameterName, "Paramter name");
        }

        [TestCase("goo")]
        [TestCase("")]
        public void When__MasterProperty_HasValue__And_Target_HasValue__Must_Pass(string value)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "good value" } },
                { "dependency", new [] { value } }
            };

            var result = ParameterReader.GetParameters<TestParamsAllowEmpty>(argvs);

            Assert.AreEqual(value, result.Dependency, "Target");
            Assert.AreEqual("good value", result.Master, "Master");
        }

        [Test]
        public void When__MasterProperty_HasNoValue__And_Target_NoValue__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>();

            var result = ParameterReader.GetParameters<TestParamsAllowEmpty>(argvs);

            Assert.IsNull(result.Dependency, "Target");
            Assert.IsNull(result.Master, "Master");
        }

        [Test]
        public void When__MasterProperty_HasNoValue__And_Target_HasValue__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "dependency", new [] { "wazzaa!" } }
            };

            var result = ParameterReader.GetParameters<TestParamsAllowEmpty>(argvs);

            Assert.AreEqual("wazzaa!", result.Dependency, "Target");
            Assert.IsNull(result.Master, "Master");
        }

        [Test]
        public void When__MasterProperty_HasValue__And_Target_NoValue_And_OtherGroupMember_HasValue__Must_Pass()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "good value" } },
                { "dependency2", new [] { "wazzaa!" } }
            };

            var result = ParameterReader.GetParameters<TestParamsWithEitherOr>(argvs);

            Assert.AreEqual("good value", result.Master, "Master");
            Assert.AreEqual("wazzaa!", result.DependencySubstitute, "Target Substitute");
            Assert.IsNull(result.Dependency, "Target");
        }

        [Test]
        public void When__MasterProperty_HasValue__And_Target_HasValue_And_OtherGroupMember_HasNoValue__Must_Pass()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "good value" } },
                { "dependency1", new [] { "wazzaa!" } }
            };

            var result = ParameterReader.GetParameters<TestParamsWithEitherOr>(argvs);

            Assert.AreEqual("good value", result.Master, "Master");
            Assert.IsNull(result.DependencySubstitute, "Target Substitute");
            Assert.AreEqual("wazzaa!", result.Dependency, "Target");
        }


        [Test]
        public void When__MasterProperty_HasValue__And_Target_NoValue_And_OtherGroupMember_NoValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "good value" } }
            };

            Assert.Throws<ParameterGroupException>(() => ParameterReader.GetParameters<TestParamsWithEitherOr>(argvs));
        }

        [Test]
        public void When__Master_Has_Two_Dependencies__And_Both_HaveValues__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "alright, partner" } },
                { "dependency1", new [] { "you know what time it is" } },
                { "dependency2", new [] { "keep on rollin'" } }
            };

            var result = ParameterReader.GetParameters<TestParamsTwoDependencies>(argvs);

            Assert.AreEqual("alright, partner", result.Master, "Master");
            Assert.AreEqual("you know what time it is", result.Dependency1, "Dependency 1");
            Assert.AreEqual("keep on rollin'", result.Dependency2, "Dependency 2");
        }

        [Test]
        public void When__Master_Has_Two_Dependencies__And_One_HasNoValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "alright, partner" } },
                { "dependency2", new [] { "keep on rollin'" } }
            };

            var exception = Assert.Throws<ParameterMissingException>(() => ParameterReader.GetParameters<TestParamsTwoDependencies>(argvs));
            Assert.AreEqual("dependency1", exception.ParameterName, "Paramter name");
        }

        class TestParams
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master))]
            public string Dependency { get; set; }
        }

        class TestParamsAllowEmpty
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master))]
            [AllowEmpty]
            public string Dependency { get; set; }
        }

        class TestParamsWithEitherOr
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency1")]
            [RequiredWith(nameof(Master))]
            [EitherOr("depenencyGroup1")]
            public string Dependency { get; set; }

            [Parameter("dependency2")]
            [EitherOr("depenencyGroup1")]
            public string DependencySubstitute { get; set; }
        }

        class TestParamsTwoDependencies
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency1")]
            [RequiredWith(nameof(Master))]
            public string Dependency1 { get; set; }

            [Parameter("dependency2")]
            [RequiredWith(nameof(Master))]
            public string Dependency2 { get; set; }
        }
    }
}