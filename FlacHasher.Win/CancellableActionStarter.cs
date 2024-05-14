using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Andy.FlacHash.Win
{
    /// <summary>
    /// An action that honors a cancellation token and runs a supplied <see cref="finishedCallback"/> when it's done
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="finishedCallback">An action that's run when the main action is finished</param>
    public delegate Task BeginCancellableAction(CancellationToken cancellationToken, Action finishedCallback, Action<Exception> failureCallback);

    public static class CancellableActionStarter
    {
        /// <summary>
        /// Starts a cancellable action and returns a cancellation token source for said action
        /// </summary>
        /// <param name="beginCancellableAction">An action that begins an action into which a cancellation token is injected</param>
        /// <param name="finishedCallback">A callback that runs when an action in question finishes</param>
        public static (Task, CancellationTokenSource) Start(
            BeginCancellableAction beginCancellableAction,
            Action finishedCallback,
            Action<Exception> failureCallback)
        {
            if (beginCancellableAction == null) throw new ArgumentNullException(nameof(beginCancellableAction));
            if (finishedCallback == null) throw new ArgumentNullException(nameof(finishedCallback));
            if (failureCallback == null) throw new ArgumentNullException(nameof(failureCallback));

            var cancellationTokenSource = new CancellationTokenSource();

            var task = beginCancellableAction(cancellationTokenSource.Token, finishedCallback, failureCallback);

            return (task, cancellationTokenSource);
        }
    }
}