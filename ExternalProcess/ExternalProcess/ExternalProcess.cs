using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Andy.ExternalProcess
{
    /// <summary>
    /// An abstraction of <see cref="Process"/>.
    /// If Disposed of while still running, it doesn't actually exit and streams do not get killed - everything continues working as normal.
    /// </summary>
    public interface IExternalProcess : IDisposable
    {
        bool Start();
        bool WaitForExit(int timeoutMs);

        /// <summary>
        /// Forces the process to exit, which results in the streams getting closed/disposed of
        /// after first writing EOF to stdout and stderr
        /// </summary>
        void Kill(bool entireProcessTree);
        bool HasExited { get; }
        int ExitCode { get; }

        StreamReader StandardOutput { get; }
        StreamWriter StandardInput { get; }
        StreamReader StandardError { get; }
    }

    public class ExternalProcess : Process, IExternalProcess
    {

    }
}