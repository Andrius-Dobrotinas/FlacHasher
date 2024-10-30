using Andy.FlacHash.Application.Audio;
using Andy.FlacHash.Audio;
using Andy.FlacHash.Hashing;
using Andy.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andy.FlacHash.Application.Win
{
    public class HasherFactory
    {
        private readonly ExternalProcess.ProcessRunner processRunner;
        private readonly FileReadProgressReporter fileReadProgressReporter;
        private readonly Settings settings;

        public HasherFactory(ExternalProcess.ProcessRunner processRunner, FileReadProgressReporter fileReadProgressReporter, Settings settings)
        {
            this.processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
            this.fileReadProgressReporter = fileReadProgressReporter ?? throw new ArgumentNullException(nameof(fileReadProgressReporter));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IReportingMultiFileHasher BuildDecoder(FileInfo decoderExe, string[] decoderParameters)
        {
            var decoderParams = AudioDecoder.GetDefaultDecoderParametersIfEmpty(decoderParameters, decoderExe);
            var decoder = AudioDecoder.Build(
                decoderExe,
                processRunner,
                decoderParams,
                fileReadProgressReporter);

            var hasher = new FileHasher(
                decoder,
                new Crypto.Hasher(settings.HashAlgorithm));
            var cancellableHasher = new ReportingMultiFileHasher(
                new MultiFileHasher(
                    hasher,
                    continueOnError: !settings.FailOnError,
                    decoder is StdInputStreamAudioFileDecoder ? null : fileReadProgressReporter));

            return cancellableHasher;
        }
    }
}