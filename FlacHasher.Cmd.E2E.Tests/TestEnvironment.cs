using System.Reflection;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    static class TestEnvironment
    {
        public const string DecoderVariableName = "FLACHASH_TEST_DECODER_FLAC";
        public const string ApeDecoderVariableName = "FLACHASH_TEST_DECODER_APE";

        const string appExecutableName = "FlacHasher";
        const string fakeDecoderExecutableName = "FakeDecoder";

        public static FileInfo AppExecutable
        {
            get
            {
                var directory = GetAssemblyMetadata("AppOutputDirectory");
                var fileName = OperatingSystem.IsWindows() ? $"{appExecutableName}.exe" : appExecutableName;
                var file = new FileInfo(Path.Combine(directory, fileName));

                if (!file.Exists)
                    throw new FileNotFoundException($"The application executable hasn't been found. Build {appExecutableName} first.", file.FullName);

                return file;
            }
        }

        /// <summary>
        /// A fake decoder whose behavior and output can be controlled by the test, run as a real process.
        /// </summary>
        public static FileInfo GetFakeDecoder()
        {
            var directory = GetAssemblyMetadata("FakeDecoderOutputDirectory");
            var fileName = OperatingSystem.IsWindows() ? $"{fakeDecoderExecutableName}.exe" : fakeDecoderExecutableName;
            var file = new FileInfo(Path.Combine(directory, fileName));

            if (!file.Exists)
                throw new FileNotFoundException($"The fake decoder's executable hasn't been found. Build {fakeDecoderExecutableName} first.", file.FullName);

            return file;
        }

        public static FileInfo GetFlacDecoder()
        {
            var path = Environment.GetEnvironmentVariable(DecoderVariableName);
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception($"Provide FLAC decoder's full path via {DecoderVariableName} env variable");

            var file = new FileInfo(path);

            if (!file.Exists)
                throw new Exception($"FLAC decoder does not exist at the specified path: {path}");

            return file;
        }

        public static FileInfo GetApeDecoder()
        {
            var path = Environment.GetEnvironmentVariable(ApeDecoderVariableName);
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception($"Provide APE decoder's full path via {ApeDecoderVariableName} env variable");
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new Exception($"APE decoder does not exist at the specified path: {path}");

            return file;
        }

        public static FileInfo GetTestAsset(string fileName)
        {
            var file = new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, SampleAsset.Directory, fileName));

            if (!file.Exists)
                throw new FileNotFoundException($"Test asset not found. Generate it with TestAssets/make-test-assets.ps1.", file.FullName);

            return file;
        }

        /// <summary>
        /// Creates a fresh temp directory with a settings file for the application to run in.
        /// </summary>
        public static DirectoryInfo SetUpWorkingDirWithSettingsFile(string settingsFileContent = "")
        {
            var workingDirectory = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), $"flachash-e2e-{Guid.NewGuid():N}"));

            File.WriteAllText(Path.Combine(workingDirectory.FullName, "settings.ini"), settingsFileContent);

            return workingDirectory;
        }

        static string GetAssemblyMetadata(string key)
        {
            return Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .First(x => x.Key == key)
                .Value;
        }
    }
}
