using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FlacHash.Win
{
    /// <summary>
    /// Runs actions, one at a time, in dedicated new threads and provides the means to cancel them. Reports transitions between action states (ie running vs finished).
    /// </summary>
    public class CancellableActionRunner
    {
        /// <summary>
        /// Fired when an action finishes, be it due to cancelation or natural completion
        /// </summary>
        public delegate void CompletionHandler(bool cancelled);

        /// <summary>
        /// Fired when an action starts and when it finishes
        /// </summary>
        /// <param name="inProgress">Indicates what state the action has transitioned to</param>
        public delegate void StateChangeHandler(bool inProgress);

        private readonly CompletionHandler reportCompletion;
        private readonly StateChangeHandler stateChanged;

        private CancellationTokenSource cancellationTokenSource;
        private bool inProgress = false;

        /// <summary>
        /// Indicates whether there currently is an operation in progress
        /// </summary>
        public bool InProgress => inProgress;

        /// <param name="reportCompletion">Fired when an action finishes, be it due to cancelation or natural completion</param>
        /// <param name="stateChanged">Fired when an action starts and when it finishes</param>
        public CancellableActionRunner(CompletionHandler reportCompletion, StateChangeHandler stateChanged)
        {
            this.reportCompletion = reportCompletion ?? throw new ArgumentNullException(nameof(reportCompletion));
            this.stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
        }

        private void ToggleActionState(bool inProgress)
        {
            this.inProgress = inProgress;
            stateChanged(inProgress);
        }

        private void OnFinished()
        {
            ToggleActionState(false);

            reportCompletion(cancellationTokenSource.IsCancellationRequested);

            cancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Starts a new action using the currently set up event handlers.
        /// Won't start a new action if there is another action in progress.
        /// </summary>
        public Task Start(BeginCancellableAction beginCancellableAction)
        {
            if (inProgress)
                throw new InvalidOperationException("More than one action may not be run at a time");

            ToggleActionState(true);

            var result = CancellableActionStarter.Start(beginCancellableAction, OnFinished);

            cancellationTokenSource = result.Item2;

            return result.Item1;
        }

        /// <summary>
        /// Cancels a currently running action
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
    }
}