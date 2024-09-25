using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_Bool_Tests
    {
        [Test]
        public void When__MasterProperty_IsTrue__And_Target_NoValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "true" } }
            };

            var exception = Assert.Throws<ParameterDependencyUnmetException>(() => ParameterReader.GetParameters<TestParams>(argvs));
            Assert.AreEqual(nameof(TestParams.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [Test]
        public void When__Nullable_MasterProperty_IsTrue__And_Target_NoValue__Must_Reject()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "true" } }
            };

            var exception = Assert.Throws<ParameterDependencyUnmetException>(() => ParameterReader.GetParameters<TestParamsNullable>(argvs));
            Assert.AreEqual(nameof(TestParamsNullable.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [TestCase(true, "goo")]
        [TestCase(false, "goo")]
        public void When__MasterProperty_HasValue__And_Target_HasValue__Must_Pass(bool masterValue, string value)
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { masterValue.ToString() } },
                { "dependency", new [] { value } }
            };

            var result = ParameterReader.GetParameters<TestParams>(argvs);

            Assert.AreEqual(value, result.Dependency, "Target");
            Assert.AreEqual(masterValue, result.Master, "Master");
        }

        [Test]
        public void When__MasterProperty_IsFalse__And_Target_NoValue__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "master", new [] { "false" } }
            };

            var result = ParameterReader.GetParameters<TestParamsNullable>(argvs);

            Assert.IsNull(result.Dependency, "Target");
            Assert.IsFalse(result.Master, "Master");
        }

        [Test]
        public void When__Nullable_MasterProperty_HasNoValue__And_Target_NoValue__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>();

            var result = ParameterReader.GetParameters<TestParamsNullable>(argvs);

            Assert.IsNull(result.Dependency, "Target");
            Assert.IsNull(result.Master, "Master");
        }

        [Test]
        public void When__Nullable_MasterProperty_HasNoValue__And_Target_HasValue__Must_BeCool()
        {
            var argvs = new Dictionary<string, string[]>()
            {
                { "dependency", new [] { "wazzaa!" } }
            };

            var result = ParameterReader.GetParameters<TestParamsNullable>(argvs);

            Assert.AreEqual("wazzaa!", result.Dependency, "Target");
            Assert.IsNull(result.Master, "Master");
        }

        class TestParams
        {
            [Parameter("master")]
            [Optional]
            public bool Master { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master))]
            public string Dependency { get; set; }
        }

        class TestParamsNullable
        {
            [Parameter("master")]
            public bool? Master { get; set; }

            [Parameter("dependency")]
            [RequiredWithAttribute(nameof(Master))]
            public string Dependency { get; set; }
        }
    }
}