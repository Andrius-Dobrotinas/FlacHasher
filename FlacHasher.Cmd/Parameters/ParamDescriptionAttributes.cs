using Andy.Cmd.Parameter;
using Andy.FlacHash.Audio;

namespace Andy.FlacHash.Application.Cmd.Parameters
{
    public class DecoderExeDescriptionAttribute : ParameterDescriptionAttribute
    {
        public DecoderExeDescriptionAttribute()
            : base("Path to the Audio decoder executable file")
        {
        }
    }

    public class DecoderParamsDescriptionAttribute : ParameterDescriptionAttribute
    {
        public DecoderParamsDescriptionAttribute()
            : base($"An array of parameters to the Audio decoder (to process a single file), exactly the way they are supposed to appear (with dashes and whatnot), but separated by semi-colons instead of spaces. Filename placeholder: \"{DecoderParameter.FilePlaceholder}\"; alternatively, data can be fed via stdin - use the approrpiate decoder parameter for that. If not specified, default FLAC parameters are used, but this HAS to be specified for other decoders")
        {
        }
    }

    public class DecoderTargetFileExtensionsAttribute : ParameterDescriptionAttribute
    {
        public DecoderTargetFileExtensionsAttribute()
            : base("File types that are accepted by the configured Audio decoder (semi-colon-separated). Only for file lookup when specifying an input Directory. This can be stored with each Audio Decoder profile")
        {
        }
    }
}
