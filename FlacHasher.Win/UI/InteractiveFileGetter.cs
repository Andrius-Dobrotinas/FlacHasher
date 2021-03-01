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
        private readonly string sourceFileFilter;
        private readonly string hashFileFilter;

        public InteractiveFileGetter(
            InteractiveDirectoryFileGetter dirBrowser,
            string sourceFileFilter,
            string hashFileFilter)
        {
            this.dirBrowser = dirBrowser;
            this.sourceFileFilter = sourceFileFilter;
            this.hashFileFilter = hashFileFilter;
        }

        /// <summary>
        /// Shows a file dialog and either returns a list of files or null if the operation is cancelled by the user
        /// </summary>
        public (FileInfo[], FileInfo)? GetFiles()
        {
            var result = dirBrowser.GetDirectory();
            if (result == null) return null;

            var files = IOUtil
                .FindFiles(result, sourceFileFilter)
                .ToArray();

            var hashFile = IOUtil
                .FindFiles(result, hashFileFilter)
                .FirstOrDefault();

            return (files, hashFile);
        }

        public void Dispose()
        {
            dirBrowser.Dispose();
        }
    }
}