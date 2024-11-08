using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win.UI
{
    /// <summary>
    /// Disposes of its dependencies
    /// </summary>
    public class InteractiveDirectoryGetter : IDisposable
    {
        private readonly FolderBrowserDialog dirBrowser;

        public InteractiveDirectoryGetter(
            FolderBrowserDialog dirBrowser)
        {
            this.dirBrowser = dirBrowser;
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