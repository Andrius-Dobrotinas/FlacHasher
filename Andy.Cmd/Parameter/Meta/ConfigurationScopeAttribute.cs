namespace Andy.Cmd.Parameter.Meta
{
    public class ConfigurationScopeAttribute : Attribute
    {
        public enum ConfigurationScope
        {
            Global = 0,
            /// <summary>
            /// The parameter applies to an individual instance of an operation, rather than being global
            /// </summary>
            OperationInstance = 1
        }

        public ConfigurationScope Scope { get; }

        protected ConfigurationScopeAttribute(ConfigurationScope scope)
        {
            Scope = scope;
        }
    }
}