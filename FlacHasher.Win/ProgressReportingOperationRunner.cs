using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public class ProgressReportingOperationRunner
    {
        public delegate void ProgressReportingOperation<TProgress>(Action<TProgress> reportProgress);

        /// <summary>
        /// Runs a given operation in a new thread, but does the progress reporting in a specified context (that is tied to a thread)
        /// </summary>
        /// <typeparam name="TProgress">Type of object that represents progress of the operation</typeparam>
        /// <param name="operation">An operation that is started in a new thread</param>
        /// <param name="progressReportingContext">A context (thread) in which the reporting of process is to be carried out</param>
        /// <param name="reportProgressInContext">An action that reports progress of the opration</param>
        /// <param name="reportCompletionInContext">An action that reports the completion of the operation</param>
        public Task StartOnNewThread<TProgress>(
            ProgressReportingOperation<TProgress> operation,
            Control progressReportingContext,
            Action<TProgress> reportProgressInContext,
            Action reportCompletionInContext,
            Action<Exception> reportFailure)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            if (progressReportingContext == null) throw new ArgumentNullException(nameof(progressReportingContext));
            if (reportProgressInContext == null) throw new ArgumentNullException(nameof(reportProgressInContext));
            if (reportCompletionInContext == null) throw new ArgumentNullException(nameof(reportCompletionInContext));
            if (reportFailure == null) throw new ArgumentNullException(nameof(reportFailure));

            void ReportProgress_OnUiThread(TProgress result)
            {
                progressReportingContext.Invoke(new Action(() => reportProgressInContext(result)));
            }

            void RunPostAction_OnUiThread(Task task)
            {
                if (task.IsFaulted)
                    progressReportingContext.Invoke(reportFailure, task.Exception);
                else
                    progressReportingContext.Invoke(reportCompletionInContext);
            };

            return Task.Factory
                .StartNew(() =>
                {
                    operation(ReportProgress_OnUiThread);
                })
                .ContinueWith(RunPostAction_OnUiThread);
        }
    }
}