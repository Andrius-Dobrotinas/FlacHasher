using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterResolution_Tests
{
    public class EitherOr_Tests : EitherOrTests_Base<TestParamsEitherOr>
    {

    }
    
    public class TestParamsEitherOr : TestParams
    {
        [Parameter("param")]
        [EitherOr("group")]
        public override string Target { get; set; }

        [Parameter("param")]
        [EitherOr("group")]
        [AllowEmpty]
        public override string AllowEmpty { get; set; }

        [Parameter("param")]
        [RequiredWith("master")]
        [Optional(defaultValue: "something")]
        public override string Optional_DefaultValue { get; set; }
    }
}