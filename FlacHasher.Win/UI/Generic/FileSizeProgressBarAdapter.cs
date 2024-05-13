using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class FileSizeProgressBarAdapter
    {
        /* todo: think about what to do with this. technically, this should be a value
         * that guarantees that the result doesn't exceed int size... but then the real-world
         * values (file sizes) could be too small? think about this at some point */
        private const int bytesPerKb = 1024;

        private readonly ProgressBar progressBar;

        public FileSizeProgressBarAdapter(ProgressBar progressBar)
        {
            this.progressBar = progressBar;
        }

        public void Increment(long bytesRead)
        {
            long progress = bytesRead / bytesPerKb;

            progressBar.Increment((int)progress);
        }

        public void Reset(long maxValue)
        {
            progressBar.Value = 0;

            long maximum = maxValue / bytesPerKb;
            progressBar.Maximum = (int)maximum;
        }

        public void SetToMax()
        {
            progressBar.Value = progressBar.Maximum;
        }
    }
}