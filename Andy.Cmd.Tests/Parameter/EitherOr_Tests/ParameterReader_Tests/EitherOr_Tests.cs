using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterReader_Tests
{
    public class EitherOr_Tests : EitherOrTests_Base<TestParamsEitherOr, TestParamsDifferentKeysEitherOr>
    {
        [Test]
        public void Neither_Parameter_HasValue__Must_Reject()
        {
            var property1 = typeof(TestParamsEitherOr).GetProperty(nameof(TestParams.One));
            var property2 = typeof(TestParamsEitherOr).GetProperty(nameof(TestParams.Two));
            var property3 = typeof(TestParamsEitherOr).GetProperty(nameof(TestParams.Three));
            Set_ParameterValueResolver_Up<TestParamsEitherOr>(property1, null);
            Set_ParameterValueResolver_Up<TestParamsEitherOr>(property2, null);
            Set_ParameterValueResolver_Up<TestParamsEitherOr>(property3, null);

            Assert.Throws<ParameterGroupException>(
                () => target.GetParameters<TestParamsEitherOr>(fakeArgs.Object));
        }
    }


    public class TestParamsEitherOr : TestParams
    {
        [Parameter("arg1")]
        [EitherOr("key1")]
        public override string One { get; set; }

        [Parameter("arg2")]
        [EitherOr("key1")]
        public override int? Two { get; set; }

        [Parameter("arg3")]
        [EitherOr("key1")]
        public override string[] Three { get; set; }
    }

    public class TestParamsDifferentKeysEitherOr : TestParamsDifferentKeys
    {
        [Parameter("arg1")]
        [EitherOr("key1")]
        public override string One { get; set; }

        [Parameter("arg2")]
        [EitherOr("key2")]
        public override string[] Two { get; set; }
    }
}