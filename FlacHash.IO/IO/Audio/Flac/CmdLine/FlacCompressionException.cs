﻿using System;
using System.Collections.Generic;

namespace Andy.FlacHash.IO.Audio.Flac.CmdLine
{
    public class FlacCompressionException : AudioCompressionException
    {
        public FlacCompressionException(
            string msg,
            ExternalProcess.ExecutionException e)
            : base($"FLAC process exited with error code {e.ExitCode}.\nFLAC error output: {e.ProcessErrorOutput}")
        {

        }
    }
}
