using System.Reflection;
using NUnit.Framework;

namespace Andy.FlacHash.Application.Cmd.E2E
{
    static class TestEnvironment
    {
        public const string DecoderVariableName = "FLACHASH_TEST_DECODER";

        const string appExecutableName = "FlacHasher";

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
        /// A real audio decoder is an external dependency that can't be shipped with the source code,
        /// so its location has to be supplied by whoever runs the tests.
        /// </summary>
        public static FileInfo GetDecoderOrFailTest()
        {
            var path = Environment.GetEnvironmentVariable(DecoderVariableName);

            var file = new FileInfo(path);

            if (!file.Exists)
                Assert.Fail($"An audio decoder is required to run this test. Set {DecoderVariableName} to the full path of a FLAC decoder executable; none was found at the fallback location {file.FullName}");

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
