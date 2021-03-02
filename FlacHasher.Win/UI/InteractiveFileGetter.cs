using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    /// <summary>
    /// Disposes of its dependencies
    /// </summary>
    public class InteractiveFileGetter
    {
        private readonly InteractiveDirectoryFileGetter dirBrowser;
        private readonly string targetFileExtension;
        private readonly string hashFileExtension;

        public InteractiveFileGetter(
            InteractiveDirectoryFileGetter dirBrowser,
            string targetFileExtension,
            string hashFileExtension)
        {
            this.dirBrowser = dirBrowser;
            this.targetFileExtension = targetFileExtension;
            this.hashFileExtension = hashFileExtension;
        }

        /// <summary>
        /// Shows a file dialog and either returns a list of files or null if the operation is cancelled by the user
        /// </summary>
        public (FileInfo[], FileInfo)? GetFiles()
        {
            var directory = dirBrowser.GetDirectory();
            if (directory == null) return null;

            var allFiles = IOUtil.FindFiles(directory, new string[] { targetFileExtension, hashFileExtension })
                .GroupBy(x => x.Extension)
                .ToArray()
                .ToDictionary(x => x.Key, x => x.ToArray());

            var files = allFiles.ContainsKey(targetFileExtension)
                ? allFiles[targetFileExtension]
                : new FileInfo[0];

            var hashFile = allFiles.ContainsKey(hashFileExtension)
                ? allFiles[hashFileExtension].First()
                : null;

            return (files, hashFile);
        }

        public void Dispose()
        {
            dirBrowser.Dispose();
        }
    }
}