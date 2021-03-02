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
        private readonly TargetFileResolver targetFileResolver;

        public InteractiveFileGetter(
            InteractiveDirectoryFileGetter dirBrowser,
            TargetFileResolver targetFileResolver)
        {
            this.dirBrowser = dirBrowser;
            this.targetFileResolver = targetFileResolver;
        }

        /// <summary>
        /// Shows a file dialog and either returns a list of files or null if the operation is cancelled by the user
        /// </summary>
        public (FileInfo[], FileInfo)? GetFiles()
        {
            var directory = dirBrowser.GetDirectory();
            if (directory == null) return null;

            return targetFileResolver.GetFiles(directory);
        }

        public void Dispose()
        {
            dirBrowser.Dispose();
        }
    }
}