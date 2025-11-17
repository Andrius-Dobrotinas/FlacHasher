using Andy.Cmd.Parameter.Meta;

namespace Andy.FlacHash.Application
{
    /// <summary>
    /// Indicates that the parameter is instance-scoped, applies to an individual instance of an operation, rather than being global
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OperationInstanceConfigurationAttribute : ConfigurationScopeAttribute
    {
    }
}