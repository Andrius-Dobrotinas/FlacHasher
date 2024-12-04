using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_EitherOr_Combo_ParameterReader_Tests
    {
        ParameterReader target;
        Mock<IParameterValueResolver> resolver;
        Mock<IDictionary<string, string[]>> fakeArgs;

        [SetUp]
        public void SetUp()
        {
            resolver = new Mock<IParameterValueResolver>();
            target = new ParameterReader(resolver.Object);

            fakeArgs = new Mock<IDictionary<string, string[]>>();
        }

        [TestCase("wazzaa!", null)]
        [TestCase(null, "xyz")]
        [TestCase("", null)]
        [TestCase(null, "")]
        public void MasterProperty_HasValue__And_One_Of_Dependencies_HasValue__Must_Pass(string targetPropertyValue, string otherEitherOrGroupMember_Value)
        {
            var masterProperty = typeof(TestParamsWithEitherOr).GetProperty(nameof(TestParamsWithEitherOr.Master));
            var property1 = typeof(TestParamsWithEitherOr).GetProperty(nameof(TestParamsWithEitherOr.Dependency));
            var property2 = typeof(TestParamsWithEitherOr).GetProperty(nameof(TestParamsWithEitherOr.DependencySubstitute));
            Util.Set_ParameterValueResolver_Up<TestParamsWithEitherOr>(resolver, masterProperty, "Master! Master!");
            Util.Set_ParameterValueResolver_Up<TestParamsWithEitherOr>(resolver, property1, targetPropertyValue);
            Util.Set_ParameterValueResolver_Up<TestParamsWithEitherOr>(resolver, property2, otherEitherOrGroupMember_Value);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParamsWithEitherOr>(fakeArgs.Object));
        }

        [TestCase("Master of puppets is pulling the strings")]
        [TestCase("Obey your master! master!")]
        public void MasterProperty_HasValue__And_Neither_Of_Dependencies_HasValue__Must_Reject(string masterValue)
        {
            var masterProperty = typeof(TestParamsWithEitherOr).GetProperty(nameof(TestParamsWithEitherOr.Master));
            Util.Set_ParameterValueResolver_Up<TestParamsWithEitherOr>(resolver, masterProperty, masterValue);

            Assert.Throws<ParameterGroupException>(() =>
                target.GetParameters<TestParamsWithEitherOr>(fakeArgs.Object));
        }

        [TestCase("whatever", "again", "either or substitute")]
        [TestCase("what a waste", null, "go back to the office")]
        [TestCase(null, "death valley", "five chords")]
        public void PropertyHas__Two_MasterProperties__MastersHaveValues__And_Target_Belongs_To_EitherOrGroup__And_Has_NoValue__But_The_Other_EitherOrGroup_Memeber_HasValue__Must_Pass(
            string master1, string master2, string other)
        {
            var masterProperty1 = typeof(TestParamsTwoMasterWithEitherOr).GetProperty(nameof(TestParamsTwoMasterWithEitherOr.Master1));
            var masterProperty2 = typeof(TestParamsTwoMasterWithEitherOr).GetProperty(nameof(TestParamsTwoMasterWithEitherOr.Master2));
            var property4 = typeof(TestParamsTwoMasterWithEitherOr).GetProperty(nameof(TestParamsTwoMasterWithEitherOr.Dependency2));
            Util.Set_ParameterValueResolver_Up<TestParamsTwoMasterWithEitherOr>(resolver, masterProperty1, master1);
            Util.Set_ParameterValueResolver_Up<TestParamsTwoMasterWithEitherOr>(resolver, masterProperty2, master2);
            Util.Set_ParameterValueResolver_Up<TestParamsTwoMasterWithEitherOr>(resolver, property4, other);

            Assert.DoesNotThrow(() => target.GetParameters<TestParamsTwoMasterWithEitherOr>(fakeArgs.Object));
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

        class TestParamsTwoMasterWithEitherOr
        {
            [Parameter("master1")]
            [Optional]
            public string Master1 { get; set; }

            [Parameter("master2")]
            [Optional]
            public string Master2 { get; set; }

            [Parameter("dependency1")]
            [RequiredWith(nameof(Master1))]
            [RequiredWith(nameof(Master2))]
            [EitherOr("group1")]
            public string Dependency1 { get; set; }

            [Parameter("dependency2")]
            [EitherOr("group1")]
            public string Dependency2 { get; set; }
        }
    }
}