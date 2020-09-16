using System;
using System.Collections.Generic;
using System.Threading;

namespace Andy.FlacHash.Win
{
    /// <summary>
    /// An action that honors a cancellation token and runs another action when it's done
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="finishedCallback">An action that's run when the main action is finished</param>
    public delegate void BeginCancellableAction(CancellationToken cancellationToken, Action finishedCallback);

    public static class CancellableActionStarter
    {
        /// <summary>
        /// Starts a cancellable action and returns a cancellation token source for said action
        /// </summary>
        /// <param name="beginCancellableAction">An action that begins an action into which a cancellation token is injected</param>
        /// <param name="finishedCallback">A callback that runs when an action in question finishes</param>
        public static CancellationTokenSource Start(
            BeginCancellableAction beginCancellableAction,
            Action finishedCallback)
        {
            if (beginCancellableAction == null) throw new ArgumentNullException(nameof(beginCancellableAction));
            if (finishedCallback == null) throw new ArgumentNullException(nameof(finishedCallback));

            var cancellationTokenSource = new CancellationTokenSource();

            beginCancellableAction(cancellationTokenSource.Token, finishedCallback);

            return cancellationTokenSource;
        }
    }
}