using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Andy.Cmd.Parameter.EitherOr_Tests.ParameterResolution_Tests
{
    public class EitherOrOptional_Tests : EitherOrTests_Base<TestParamsEitherOr_Optional>
    {

    }
    
    public class TestParamsEitherOr_Optional : TestParams
    {
        [Parameter("param")]
        [OptionalEitherOr("group")]
        public override string Target { get; set; }

        [Parameter("param")]
        [OptionalEitherOr("group")]
        [AllowEmpty]
        public override string AllowEmpty { get; set; }

        [Parameter("param")]
        [RequiredWith("master")]
        [Optional(defaultValue: "something")]
        public override string Optional_DefaultValue { get; set; }
    }
}