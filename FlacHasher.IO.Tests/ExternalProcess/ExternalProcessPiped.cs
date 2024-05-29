using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;

namespace Andy.FlacHash.IO.ExternalProcess
{
    class ExternalProcessPiped : ExternalProcessFake
    {
        AnonymousPipeServerStream server;
        Stream client;

        public ExternalProcessPiped(TaskCompletionSource<bool> voluntaryExitCompletion = null, bool respondToExitRequest = true, Stream stderr = null, bool closeStreamsOnExit = true)
            : base(stdout: null, stdin: null, stderr: stderr, voluntaryExitCompletion: voluntaryExitCompletion,  respondToExitRequest: respondToExitRequest, closeStreamsOnExit: closeStreamsOnExit)
        {
            server = new AnonymousPipeServerStream(PipeDirection.Out);
            client = new StdoutStream(
                new AnonymousPipeClientStream(PipeDirection.In, server.ClientSafePipeHandle));

            StandardOutput = new StreamReader(client);
            StandardInput = new StreamWriter(server);
        }

        public ExternalProcessPiped(AnonymousPipeServerStream stdoutSource, TaskCompletionSource<bool> voluntaryExitCompletion = null, bool respondToExitRequest = true, Stream stderr = null, bool closeStreamsOnExit = true)
            : base(stdout: null, stdin: null, stderr: stderr, voluntaryExitCompletion: voluntaryExitCompletion, respondToExitRequest: respondToExitRequest, closeStreamsOnExit: closeStreamsOnExit)
        {
            client = new StdoutStream(
                new AnonymousPipeClientStream(PipeDirection.In, stdoutSource.ClientSafePipeHandle));

            StandardOutput = new StreamReader(client);
        }

        public override void Destroy()
        {
            base.Destroy();
            client.Dispose();
            server.Dispose();
        }
    }
}
