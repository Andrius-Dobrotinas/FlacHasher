using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Application.Win
{
    /// <summary>
    /// Runs actions, one at a time, on dedicated new threads and provides the means to cancel them. Reports transitions between action states (ie running vs finished).
    /// </summary>
    public class CancellableBackgroundOperationRunner
    {
        /// <summary>
        /// A cancellable operation that reports its progress
        /// </summary>
        public delegate void ReportableOperation<TUpdate>(Action<TUpdate> reportUpdate, CancellationToken cancellationToken);

        /// <summary>
        /// Fired when an action finishes, be it due to cancelation or natural completion
        /// </summary>
        public delegate void CompletionHandler(bool cancelled);
        
        /// <summary>
        /// Fired when an action starts and when it finishes
        /// </summary>
        /// <param name="inProgress">Indicates what state the action has transitioned to</param>
        public delegate void StateChangeHandler(bool inProgress);

        private CancellationTokenSource cancellation;
        private bool inProgress = false;
        private readonly Control uiContext;

        /// <summary>
        /// Indicates whether there currently is an operation in progress
        /// </summary>
        public bool InProgress => inProgress;

        /// <param name="uiContext">Control on whose context the callbacks (action's communication) are to be invoked</param>
        public CancellableBackgroundOperationRunner(Control uiContext)
        {
            this.uiContext = uiContext;
        }

        /// <summary>
        /// Runs the supplied <paramref name="operation"/> on a non-UI thread, if it's currently not running anything,
        /// and invokes <paramref name="reportUpdate"/>, <paramref name="reportCompletion"/>, <paramref name="reportFailure"/> and <paramref name="stateChanged"/>
        /// callbacks on a UI thread
        /// </summary>
        /// <param name="reportUpdate">Whatever the action reports from time to time</param>
        /// <param name="reportCompletion">When the action finishes successfully or is cancelled</param>
        /// <param name="reportFailure">When the action errors out</param>
        /// <param name="stateChanged">Gets invoked whenever action transitions from running to completed</param>
        /// <exception cref="InvalidOperationException">If attempting to start an action while there's one still running</exception>
        public Task Start<TUpdate>(
            Action<TUpdate> reportUpdate,
            CompletionHandler reportCompletion,
            Action<Exception> reportFailure,
            StateChangeHandler stateChanged,
            ReportableOperation<TUpdate> operation)
        {
            if (inProgress)
                throw new InvalidOperationException("More than one action may not be run at a time");

            cancellation = new CancellationTokenSource();

            void SetRunnerState(bool inProgress)
            {
                this.inProgress = inProgress;
                stateChanged(inProgress);
            }

            SetRunnerState(true);

            return StartOnNewThread(
                operation, 
                reportUpdate,
                (exception) => // This runs in the UI context
                {
                    SetRunnerState(false);
                    if (exception != null)
                        reportFailure(exception);
                    else
                        reportCompletion(cancellation.IsCancellationRequested);
                }, 
                uiContext,
                cancellation.Token)
                .ContinueWith(t =>
                {
                    cancellation.Dispose();
                });
        }

        private static Task StartOnNewThread<TUpdate>(
            ReportableOperation<TUpdate> operation, 
            Action<TUpdate> reportUpdate,
            Action<Exception> onFinished,
            Control progressReportContext,
            CancellationToken cancellation)
        {
            return Task.Factory
                .StartNew(() =>
                {
                    operation(
                        update => progressReportContext.Invoke(new Action(() => reportUpdate(update))),
                        cancellation);
                })
                .ContinueWith(task =>
                {
                    progressReportContext.Invoke(onFinished, task.Exception);
                });
        }

        /// <summary>
        /// Cancels a currently running operation
        /// </summary>
        public void Cancel()
        {
            cancellation.Cancel();
        }
    }
}