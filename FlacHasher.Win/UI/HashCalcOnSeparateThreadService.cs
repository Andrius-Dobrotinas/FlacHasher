using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win.UI
{
    /// <summary>
    /// Calculates hash on a new thread and returns results via an event fired on a UI thread
    /// </summary>
    public class HashCalcOnSeparateThreadService
    {
        private readonly IMultipleFileHasher hasher;

        /// <summary>
        /// A control which is used as a context for UI updates from another thread
        /// </summary>
        public Control UiUpdateContext { get; set; }

        /// <summary>
        /// An event that is fired for each calculated file hash
        /// </summary>
        public event Action<FileHashResult> OnHashResultAvailable;

        public event Action OnFinished;

        public HashCalcOnSeparateThreadService(IMultipleFileHasher hasher)
        {
            this.hasher = hasher;
        }

        public void CalculateHashes(IEnumerable<FileInfo> sourceFiles)
        {
            if (UiUpdateContext == null) throw new ArgumentNullException(nameof(UiUpdateContext), "The value must be provided via the public property");

            if (OnHashResultAvailable == null) throw new ArgumentNullException(nameof(OnHashResultAvailable), "The value must be provided via the public property");

            if (OnFinished == null) throw new ArgumentNullException(nameof(OnFinished), "The value must be provided via the public property");

            Task.Factory
                .StartNew(() =>
                {
                    CalcHashesAndReportOnUIThread(sourceFiles);
                })
                .ContinueWith(ReportFinish_OnUIThread);
        }

        private void CalcHashesAndReportOnUIThread(
            IEnumerable<FileInfo> files)
        {
            IEnumerable<FileHashResult> results = hasher.ComputeHashes(files);

            foreach (var result in results)
            {
                ReportResult_OnUIThread(result);
            }
        }

        private void ReportResult_OnUIThread(FileHashResult result)
        {
            UiUpdateContext.Invoke(new Action(() => OnHashResultAvailable(result)));
        }

        private void ReportFinish_OnUIThread(Task task)
        {
            UiUpdateContext.Invoke(OnFinished);
        }
    }
}