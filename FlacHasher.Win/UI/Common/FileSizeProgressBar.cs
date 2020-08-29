using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    public class FileSizeProgressBar
    {
        private readonly ProgressBar progressBar;

        public FileSizeProgressBar(ProgressBar progressBar)
        {
            this.progressBar = progressBar;
        }

        public void Increment(long bytesRead)
        {
            long progress = bytesRead / 1024;

            progressBar.Increment((int)progress);
        }

        public void SetMaxValue(long value)
        {
            long maximum = value / 1024;
            progressBar.Maximum = (int)maximum;
        }
    }
}