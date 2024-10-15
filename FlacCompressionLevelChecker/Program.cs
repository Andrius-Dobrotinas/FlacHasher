using Andy.Cmd;
using Andy.FlacHash.CompressionLevel.Audio;
using System;
using System.IO;
using System.Linq;

namespace Andy.FlacHash.CompressionLevel
{
    class Program
    {
        const int maxCompressionLevel = (int)FlacHash.Audio.Flac.CompressionLevel.Highest;
        const int processExitTimeoutMs = 1000;
        const int processStartDelayMs = 100;
        const int processTimeoutSec = 180;

        static int Main(string[] args)
        {
            try
            {
                DoIt(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error has occured:");
                Console.WriteLine(e.Message);

                return -1;
            }

            return 0;
        }

        static void DoIt(string[] args)
        {
            FileInfo flacExe;
            FileInfo sourceFile;
            int compressionLevel;

            GetParameters(args, out flacExe, out sourceFile, out compressionLevel);

            var recoder = new FileRecoder(
                flacExe,
                new ExternalProcess.ProcessRunner(processTimeoutSec, processExitTimeoutMs, processStartDelayMs, showProcessWindowWithStdErrOutput: true));

            using (Stream recodedAudio = recoder.Encode(sourceFile, compressionLevel))
            {
                Console.WriteLine($"{sourceFile.FullName}: compressed to level {compressionLevel}: {sourceFile.Length == recodedAudio.Length}");
            }
        }

        static void GetParameters(
            string[] args,
            out FileInfo flacExec,
            out FileInfo sourceFile,
            out int compressionLevel)
        {
            var argumentDictionary = ArgumentSplitter.GetArguments(args);
            var @params = ParameterReader.GetParameters(argumentDictionary.ToDictionary(x => x.Key, x => x.Value.First()));

            if (string.IsNullOrEmpty(@params.SourceFile))
                throw new Exception("Source file not provided");

            sourceFile = new FileInfo(@params.SourceFile);
            flacExec = new FileInfo(@params.FlacExec);            
            compressionLevel = @params.CompressionLevel ?? maxCompressionLevel;
        }
    }
}