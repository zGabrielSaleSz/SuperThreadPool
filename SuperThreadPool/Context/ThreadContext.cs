namespace SuperThreadPool.Context
{
    using System.Collections.Concurrent;
    internal class ThreadContext
    {
        internal delegate void TaskQueueCountUpdate(ThreadContext context, int amountOfTasksOnQueue);
        internal event TaskQueueCountUpdate? OnTaskQueueCountUpdate;

        private readonly ConcurrentQueue<Action> _actions;
        private readonly CancellationTokenSource _cancellationToken;
        private readonly Thread _thread;
        private readonly AutoResetEvent _waitNewAction;
        private readonly AutoResetEvent _waitFinishThread;

        private int _amountOfTasksOnQueue;
        internal ThreadContext()
        {
            _cancellationToken = new CancellationTokenSource();

            _actions = new ConcurrentQueue<Action>();
            _thread = new Thread(ProcessQueue);
            _thread.Name = "zWorker";
            _waitNewAction = new AutoResetEvent(false);
            _waitFinishThread = new AutoResetEvent(false);
        }

        public void Initialize()
        {
            _thread.Start();
        }

        public void EqueueAction(Action action)
        {
            Interlocked.Increment(ref _amountOfTasksOnQueue);
            _actions.Enqueue(action);
            OnTaskQueueCountUpdate?.Invoke(this, _amountOfTasksOnQueue);
            _waitNewAction.Set();
        }

        private void ProcessQueue()
        {
            while(!_cancellationToken.Token.IsCancellationRequested)
            {
                Action? action;
                while (!_actions.TryDequeue(out action))
                {
                    _waitNewAction.WaitOne();
                } 
                action?.Invoke();
                Interlocked.Decrement(ref _amountOfTasksOnQueue);
                OnTaskQueueCountUpdate?.Invoke(this, _amountOfTasksOnQueue);
            }
            _waitFinishThread.Set();
        }

        public void Stop()
        {
            _cancellationToken.Cancel();

            // notify loop should try again
            _waitNewAction.Set();

            // waits loop finishes
            _waitFinishThread.WaitOne();

            _thread.Interrupt();

        }
    }
}
