using System;
using System.Collections.Generic;
using System.Text;

namespace Andy.FlacHash.IO.Audio.Flac
{
    public class CmdLineCompressionException : AudioCompressionException
    {
        public CmdLineCompressionException(
            string msg,
            ExternalProcess.ExecutionException e)
            : base($"FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}")
        {

        }
    }
}
