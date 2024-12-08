using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterReader_Tests.ParameterReader_Tests
{
    public class EitherOrOptional_Tests : EitherOrTests_Base<TestParamsEitherOrOptional, TestParamsDifferentKeys_OptionalEitherOr>
    {
        [Test]
        public void AllowNone__OnTheGroup__And_Both_Have_Values__Must_Reject()
        {
            var property1 = typeof(TestParams_AllowNone).GetProperty(nameof(TestParams_AllowNone.One));
            var property2 = typeof(TestParams_AllowNone).GetProperty(nameof(TestParams_AllowNone.Two));
            Set_ParameterValueResolver_Up<TestParams_AllowNone>(property1, "value");
            Set_ParameterValueResolver_Up<TestParams_AllowNone>(property2, 100);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParams_AllowNone>(fakeArgs.Object));
        }

        [Test]
        public void AllowNone__OnTheGroup__And_Neither_Has_Value__Must_BeCool()
        {
            var property1 = typeof(TestParams_AllowNone).GetProperty(nameof(TestParams_AllowNone.One));
            var property2 = typeof(TestParams_AllowNone).GetProperty(nameof(TestParams_AllowNone.Two));
            Set_ParameterValueResolver_Up<TestParams_AllowNone>(property1, null);
            Set_ParameterValueResolver_Up<TestParams_AllowNone>(property2, null);

            target.GetParameters<TestParams_AllowNone>(fakeArgs.Object);
        }

        class TestParams_AllowNone
        {
            [Parameter("arg1")]
            [OptionalEitherOr("key1")]
            public string One { get; set; }

            [Parameter("arg2")]
            [OptionalEitherOr("key1")]
            public int? Two { get; set; }
        }
    }

    public class TestParamsEitherOrOptional : TestParams
    {
        [Parameter("arg1")]
        [OptionalEitherOr("key1")]
        public override string One { get; set; }

        [Parameter("arg2")]
        [OptionalEitherOr("key1")]
        public override int? Two { get; set; }

        [Parameter("arg3")]
        [OptionalEitherOr("key1")]
        public override string[] Three { get; set; }
    }

    public class TestParamsDifferentKeys_OptionalEitherOr : TestParamsDifferentKeys
    {
        [Parameter("arg1")]
        [OptionalEitherOr("key1")]
        public override string One { get; set; }

        [Parameter("arg2")]
        [OptionalEitherOr("key2")]
        public override string[] Two { get; set; }
    }
}