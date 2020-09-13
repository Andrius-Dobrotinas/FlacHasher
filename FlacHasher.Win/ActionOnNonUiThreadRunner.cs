using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Andy.FlacHash.Win
{
    public class ActionOnNonUiThreadRunner
    {
        /// <summary>
        /// Runs a given action on a non-UI thread, but does the progress reporting and
        /// the invocation of a post-action callback on a UI thread
        /// </summary>
        /// <typeparam name="TProgress">Type of object that represents action progress</typeparam>
        /// <param name="action">An action that is invoked on a non-UI thread</param>
        /// <param name="onProgress">An action that reports progress and is invoked on a UI thread</param>
        /// <param name="onFinished">An action that's invoked on a UI thread when the main action finishes</param>
        /// <param name="uiUpdateContext">A control which will be used for running an action on a UI thread</param>
        public void Run<TProgress>(
            Action<Action<TProgress>> action,
            Action<TProgress> onProgress,
            Action onFinished,
            Control uiUpdateContext)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (uiUpdateContext == null) throw new ArgumentNullException(nameof(uiUpdateContext));
            if (onProgress == null) throw new ArgumentNullException(nameof(onProgress));
            if (onFinished == null) throw new ArgumentNullException(nameof(onFinished));

            void ReportProgress_OnUiThread(TProgress result)
            {
                uiUpdateContext.Invoke(new Action(() => onProgress(result)));
            }

            void RunPostAction_OnUiThread(Task task) => uiUpdateContext.Invoke(onFinished);

            Task.Factory
                .StartNew(() =>
                {
                    action(ReportProgress_OnUiThread);
                })
                .ContinueWith(RunPostAction_OnUiThread);
        }
    }
}