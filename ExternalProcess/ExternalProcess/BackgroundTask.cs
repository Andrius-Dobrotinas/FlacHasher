namespace Andy.ExternalProcess
{
    public static class BackgroundTask
    {
        // DenyChildAttach is used when Task.Run - keeping
        private const TaskCreationOptions BackgroundTaskOptions = TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning;

        /// <summary>
        /// Starts an action as a long-running task.
        /// Ensures the task can start immediately even if the thread pool is saturated with other long-lived tasks.
        /// </summary>
        public static Task StartBackgroundTask(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, BackgroundTaskOptions, TaskScheduler.Default);
        }

        /// <summary>
        /// Starts an action as a long-running task.
        /// Ensures the task can start immediately even if the thread pool is saturated with other long-lived tasks.
        /// </summary>
        public static Task<T> StartBackgroundTask<T>(Func<T> function)
        {
            return Task.Factory.StartNew(function, CancellationToken.None, BackgroundTaskOptions, TaskScheduler.Default);
        }
    }
}