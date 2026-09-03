using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Andy.FakeDecoder
{
    static class TestEnvironment
    {
        const string executableName = "FakeDecoder";

        /// <summary>
        /// The fake program is run as a real process, so the tests need its executable rather than its assembly.
        /// The build tells us where it ended up, via an assembly attribute.
        /// </summary>
        public static FileInfo Executable
        {
            get
            {
                var directory = GetAssemblyMetadata("FakeDecoderOutputDirectory");
                var fileName = OperatingSystem.IsWindows() ? $"{executableName}.exe" : executableName;
                var file = new FileInfo(Path.Combine(directory, fileName));

                if (!file.Exists)
                    throw new FileNotFoundException($"The fake program's executable hasn't been found. Build {executableName} first.", file.FullName);

                return file;
            }
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
