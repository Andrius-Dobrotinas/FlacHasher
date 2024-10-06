using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter
{
    public class RequiredWith_Bool_Tests
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

        [Test]
        public void When__MasterProperty_IsTrue__And_Target_NoValue__Must_Reject()
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, true);

            var exception = Assert.Throws<ParameterDependencyUnmetException>(
                () => target.GetParameters<TestParams>(fakeArgs.Object));
            Assert.AreEqual(nameof(TestParams.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [Test]
        public void When__Nullable_MasterProperty_IsTrue__And_Target_NoValue__Must_Reject()
        {
            var property1 = typeof(TestParamsNullable).GetProperty(nameof(TestParamsNullable.Master));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParamsNullable>(resolver, property1, true);

            var exception = Assert.Throws<ParameterDependencyUnmetException>(
                () => target.GetParameters<TestParamsNullable>(fakeArgs.Object));
            Assert.AreEqual(nameof(TestParamsNullable.Dependency), exception.ParameterProperty?.Name, "Paramter name");
        }

        [TestCase(true, "goo")]
        [TestCase(false, "Title tk")]
        public void When__MasterProperty_Has_AnyValue__And_Target_HasValue__Must_Pass(bool masterValue, string value)
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            var property2 = typeof(TestParams).GetProperty(nameof(TestParams.Dependency));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, masterValue);
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property2, value);

            Assert.DoesNotThrow(() => target.GetParameters<TestParams>(fakeArgs.Object));
        }

        [Test]
        public void When__MasterProperty_IsFalse__And_Target_NoValue__Must_BeCool()
        {
            var property1 = typeof(TestParams).GetProperty(nameof(TestParams.Master));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParams>(resolver, property1, false);

            Assert.DoesNotThrow(() => target.GetParameters<TestParamsNullable>(fakeArgs.Object));
        }

        [Test]
        public void When__Nullable_MasterProperty_Has_NoValue__And_Target_NoValue__Must_BeCool()
        {
            Assert.DoesNotThrow(() => target.GetParameters<TestParamsNullable>(fakeArgs.Object));
        }

        [TestCase("Anything")]
        [TestCase("You want")]
        public void When__Nullable_MasterProperty_Has_NoValue__And_Target_HasValue__Must_BeCool(string value)
        {
            var property1 = typeof(TestParamsNullable).GetProperty(nameof(TestParamsNullable.Dependency));
            EitherOr_Tests.Set_ParameterValueResolver_Up<TestParamsNullable>(resolver, property1, value);

            Assert.DoesNotThrow(() => target.GetParameters<TestParamsNullable>(fakeArgs.Object));
        }

        class TestParams
        {
            [Parameter("master")]
            [Optional]
            public bool Master { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master))]
            public string Dependency { get; set; }
        }

        class TestParamsNullable
        {
            [Parameter("master")]
            public bool? Master { get; set; }

            [Parameter("dependency")]
            [RequiredWith(nameof(Master))]
            public string Dependency { get; set; }
        }
    }
}