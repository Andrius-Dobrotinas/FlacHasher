using Andy.Cmd.Parameter.Meta;

namespace Andy.FlacHash.Application
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DecoderParamAttribute : ParameterPurposeAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OperationParamAttribute : ParameterPurposeAttribute
    {
    }
}