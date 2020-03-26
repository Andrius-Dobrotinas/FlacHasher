using System;

namespace Andy.FlacHash.Cmd
{
    /// <summary>
    /// Indicates that a command line argument has not been provided by the caller
    /// </summary>
    public class CmdLineArgNotFoundException : Exception
    {
        public CmdLineArgNotFoundException(string msg) : base(msg)
        {

        }
    }
}
