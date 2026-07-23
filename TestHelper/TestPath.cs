using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andy
{
    public static class TestPath
    {
        public static string Normalize(string path)
        {
            if (path == null)
                return null;

            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);

            if (LooksLikeWindowsAbsolutePath(path))
            {
                var drive = char.ToLowerInvariant(path[0]).ToString();
                var relativeSegments = SplitPathSegments(path.Substring(3));

                return Absolute((new[] { drive }).Concat(relativeSegments).ToArray());
            }

            if (path.Length == 2 && char.IsLetter(path[0]) && path[1] == ':')
                return Absolute(char.ToLowerInvariant(path[0]).ToString());

            return path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);
        }

        public static string Absolute(params string[] segments)
        {
            var rootedSegments = new List<string> { Path.DirectorySeparatorChar.ToString() };
            rootedSegments.AddRange(segments);

            return Path.GetFullPath(Path.Combine(rootedSegments.ToArray()));
        }

        private static bool LooksLikeWindowsAbsolutePath(string path)
        {
            return path.Length >= 3
                && char.IsLetter(path[0])
                && path[1] == ':'
                && (path[2] == '\\' || path[2] == '/');
        }

        private static string[] SplitPathSegments(string path)
        {
            return path
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar)
                .Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}