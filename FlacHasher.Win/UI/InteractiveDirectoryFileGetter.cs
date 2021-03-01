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
    public class InteractiveDirectoryFileGetter : IDisposable
    {
        private readonly FolderBrowserDialog dirBrowser;
        private readonly string sourceFileFilter;

        public InteractiveDirectoryFileGetter(
            FolderBrowserDialog dirBrowser,
            string sourceFileFilter)
        {
            this.dirBrowser = dirBrowser;
            this.sourceFileFilter = sourceFileFilter;
            dirBrowser.ShowNewFolderButton = false;
        }

        /// <summary>
        /// Shows a file dialog and either returns a list of files or null if the operation is cancelled by the user
        /// </summary>
        public DirectoryInfo GetDirectory()
        {
            var result = dirBrowser.ShowDialog();
            if (result != DialogResult.OK) return null;

            return new DirectoryInfo(dirBrowser.SelectedPath);
        }

        public void Dispose()
        {
            dirBrowser.Dispose();
        }
    }
}