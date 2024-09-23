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

            var exception = Assert.Throws<ParameterDependencyUnmetException>(() => ParameterReader.GetParameters<TestParams>(argvs));
            Assert.AreEqual(nameof(TestParams.Dependency), exception.ParameterProperty?.Name, "Paramter name");
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

            var exception = Assert.Throws<ParameterDependencyUnmetException>(() => ParameterReader.GetParameters<TestParamsTwoDependencies>(argvs));
            Assert.AreEqual(nameof(TestParamsTwoDependencies.Dependency1), exception.ParameterProperty?.Name, "Paramter name");
        }

        [Test]
        public void When__TwoMasterProperties_OneHasNoValue__And_Target_NoValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master1", new [] { "good value" } }
            };

            var exception = Assert.Throws<ParameterDependencyUnmetException>(() => ParameterReader.GetParameters<TestParamsTwoMaster>(argvs));
            Assert.AreEqual(nameof(TestParamsTwoMaster.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [Test]
        public void When__TwoMasterProperties_OneHasNoValue__And_Target_HasValue__Must_Pass()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master1", new [] { "you cool?" } },
                { "dependency", new [] { "you're cool" } },
            };

            var result = ParameterReader.GetParameters<TestParamsTwoMaster>(argvs);
            Assert.AreEqual("you cool?", result.Master1, "Master 1");
            Assert.IsNull(result.Master2, "Master 2");
            Assert.AreEqual("you're cool", result.Dependency, "Dependency");
        }

        [Test]
        public void When__TwoMasterProperties__And_Target_IsEitherOrGroup_TheOther_HasValue__Must_Pass()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master1", new [] { "you cool?" } },
                { "dependency2", new [] { "you're cool" } },
            };

            var result = ParameterReader.GetParameters<TestParamsTwoMasterWithEitherOr>(argvs);
            Assert.AreEqual("you cool?", result.Master1, "Master 1");
            Assert.IsNull(result.Master2, "Master 2");
            Assert.AreEqual("you're cool", result.Dependency2, "Dependency 2");
            Assert.IsNull(result.Dependency1, "Dependency 1");
        }

        class TestParams
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master))]
            public string Dependency { get; set; }
        }

        class TestParamsAllowEmpty
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master))]
            [AllowEmpty]
            public string Dependency { get; set; }
        }

        class TestParamsWithEitherOr
        {
            [Parameter("master")]
            [Optional]
            public string Master { get; set; }

            [Parameter("dependency1")]
            [RequiredWithAttribute(nameof(Master))]
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
            [RequiredWithAttribute(nameof(Master))]
            public string Dependency1 { get; set; }

            [Parameter("dependency2")]
            [RequiredWithAttribute(nameof(Master))]
            public string Dependency2 { get; set; }
        }

        class TestParamsTwoMaster
        {
            [Parameter("master1")]
            [Optional]
            public string Master1 { get; set; }

            [Parameter("master2")]
            [Optional]
            public string Master2 { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master1))]
            [RequiredWithAttribute(nameof(Master2))]
            public string Dependency { get; set; }
        }

        class TestParamsTwoMasterWithEitherOr
        {
            [Parameter("master1")]
            [Optional]
            public string Master1 { get; set; }

            [Parameter("master2")]
            [Optional]
            public string Master2 { get; set; }

            [Parameter("dependency1")]
            [RequiredWithAttribute(nameof(Master1))]
            [RequiredWithAttribute(nameof(Master2))]
            [EitherOr("group1")]
            public string Dependency1 { get; set; }

            [Parameter("dependency2")]
            [EitherOr("group1")]
            public string Dependency2 { get; set; }
        }
    }
}