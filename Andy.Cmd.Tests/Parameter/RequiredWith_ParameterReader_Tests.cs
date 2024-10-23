using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_ParameterReader_Tests
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

        [TestCase("good value")]
        [TestCase("")]
        public void MasterProperty_HasValue__And_Target_NoValue__Must_Reject(string value)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, value);

            var exception = Assert.Throws<ParameterDependencyUnmetException>(
                () => target.GetParameters<TestParams>(fakeArgs.Object));
            Assert.AreEqual(nameof(TestParams.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [TestCase("goo", "d value")]
        [TestCase("", "")]
        [TestCase("alright", "")]
        [TestCase("", "empty")]
        public void MasterProperty_HasValue__And_Target_HasValue__Must_Pass(string masterValue, string dependencyValue)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Dependency));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, masterValue);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, dependencyValue);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [Test]
        public void MasterProperty_Has_NoValue__And_Target_Has_NoValue__Must_BeCool()
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Dependency));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, null);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, null);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [TestCase("ladies and gentlemen!")]
        [TestCase("introducing, Limp Bizkit")]
        [TestCase("")]
        public void MasterProperty_Has_NoValue__And_Target_HasValue__Must_BeCool(string value)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Dependency));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, null);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, value);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [TestCase("obey", "your", "master")]
        [TestCase("obey", "your", "")]
        [TestCase("obey", "", "master")]
        [TestCase("", "your", "master")]
        [TestCase("", "", "")]
        [TestCase(null, "your", "master")]
        public void Master_Has_Two_Dependencies__And_Both_Dependencies_HaveValues__Must_BeCool(string master, string dependency1, string dependency2)
        {
            var masterProperty = typeof(TestParamsTwoDependencies).GetProperty(nameof(TestParamsTwoDependencies.Master));
            var property1 = typeof(TestParamsTwoDependencies).GetProperty(nameof(TestParamsTwoDependencies.Dependency1));
            var property2 = typeof(TestParamsTwoDependencies).GetProperty(nameof(TestParamsTwoDependencies.Dependency2));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoDependencies>(resolver, masterProperty, master);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoDependencies>(resolver, property1, dependency1);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoDependencies>(resolver, property2, dependency2);

            Assert.DoesNotThrow(() =>
                target.GetParameters<TestParamsTwoDependencies>(fakeArgs.Object));
        }

        [TestCase("obey", null, "master")]
        [TestCase("obey", "your", null)]
        public void Master_Has_Two_Dependencies__And_One_Dependency_Has_NoValue__Must_Reject(string master, string dependency1, string dependency2)
        {
            var masterProperty = typeof(TestParamsTwoDependencies).GetProperty(nameof(TestParamsTwoDependencies.Master));
            var property1 = typeof(TestParamsTwoDependencies).GetProperty(nameof(TestParamsTwoDependencies.Dependency1));
            var property2 = typeof(TestParamsTwoDependencies).GetProperty(nameof(TestParamsTwoDependencies.Dependency2));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoDependencies>(resolver, masterProperty, master);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoDependencies>(resolver, property1, dependency1);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoDependencies>(resolver, property2, dependency2);

            var exception = Assert.Throws<ParameterDependencyUnmetException>(
                () => target.GetParameters<TestParamsTwoDependencies>(fakeArgs.Object));
        }

        [TestCase("If I say", null)]
        [TestCase("", null)]
        [TestCase(null, "that's 46")]
        [TestCase(null, "")]
        public void PropertyHas__Two_MasterProperties__And_OneMaster_Has_NoValue__And_Target_Has_NoValue__Must_Reject(string master1, string master2)
        {
            var masterProperty1 = typeof(TestParamsTwoMaster).GetProperty(nameof(TestParamsTwoMaster.Master1));
            var masterProperty2 = typeof(TestParamsTwoMaster).GetProperty(nameof(TestParamsTwoMaster.Master2));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoMaster>(resolver, masterProperty1, master1);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoMaster>(resolver, masterProperty2, master2);

            var exception = Assert.Throws<ParameterDependencyUnmetException>(
                () => target.GetParameters<TestParamsTwoMaster>(fakeArgs.Object));
            Assert.AreEqual(nameof(TestParamsTwoMaster.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [TestCase("If I say", null, "two more times, that's 46")]
        [TestCase(null, "in this", "messed up rhyme")]
        [TestCase("If I say", null, "")]
        [TestCase(null, "in this", "")]
        public void PropertyHas__Two_MasterProperties__And_One_Master_Has_NoValue__And_Target_HasValue__Must_Pass(string master1, string master2, string dependency)
        {
            var masterProperty1 = typeof(TestParamsTwoMaster).GetProperty(nameof(TestParamsTwoMaster.Master1));
            var masterProperty2 = typeof(TestParamsTwoMaster).GetProperty(nameof(TestParamsTwoMaster.Master2));
            var property3 = typeof(TestParamsTwoMaster).GetProperty(nameof(TestParamsTwoMaster.Dependency));
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoMaster>(resolver, masterProperty1, master1);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoMaster>(resolver, masterProperty2, master2);
            EitherOr_ParameterReader_Tests.Set_ParameterValueResolver_Up<TestParamsTwoMaster>(resolver, property3, dependency);

            Assert.DoesNotThrow(() => target.GetParameters<TestParamsTwoMaster>(fakeArgs.Object));
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

        class TestParamsTwoMaster
        {
            [Parameter("master1")]
            [Optional]
            public string Master1 { get; set; }

            [Parameter("master2")]
            [Optional]
            public string Master2 { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master1))]
            [RequiredWith(nameof(Master2))]
            public string Dependency { get; set; }
        }
    }
}