namespace SuperThreadPool
{
    using SuperThreadPool.Context;
    using SuperThreadPool.Tools;
    using System.Collections.Concurrent;

    public class ThreadPool : IDisposable
    {
        private bool _isDisposed = false;
        private readonly int _threadPoolSize;
        private ConcurrentDictionary<int, ThreadContext> _threads;

        private ConcurrentQueue<Action[]> _processQueue;
        private ConcurrentQueue<ThreadContext> _threadsAvailable;

        private AutoResetEvent _waitThreadAvailable;
        private AutoResetEvent _waitProcessQueue;
        private CancellationTokenSource _cancellationToken;

        private Thread _enqueuer;

        public ThreadPool(int threadPoolSize)
        {
            Checker.CheckIsNotZero(nameof(threadPoolSize), threadPoolSize);
            _threadPoolSize = threadPoolSize;
            _cancellationToken = new CancellationTokenSource();

            _threads = new ConcurrentDictionary<int, ThreadContext>();
            _processQueue = new ConcurrentQueue<Action[]>();
            _threadsAvailable = new ConcurrentQueue<ThreadContext>();

            _waitThreadAvailable = new AutoResetEvent(false);
            _waitProcessQueue = new AutoResetEvent(false);

            Initialize();

            _enqueuer = new Thread(EnqueuerProcess);
            _enqueuer.Start();
        }

        private void EnqueuerProcess()
        {
            while (!_cancellationToken.Token.IsCancellationRequested)
            {
                Action[] actionsToProcess;
                while (!_processQueue.TryDequeue(out actionsToProcess))
                {
                    _waitProcessQueue.WaitOne();
                }
                ThreadContext threadRunner;
                while (!_threadsAvailable.TryDequeue(out threadRunner))
                {
                    _waitThreadAvailable.WaitOne();
                }
                foreach (var available in actionsToProcess)
                {
                    threadRunner.EqueueAction(available);
                }
            }
        }

        private void Initialize()
        {
            for(int i = 0; i < _threadPoolSize; i++)
            {
                ThreadContext threadContext = new ThreadContext();
                threadContext.OnTaskQueueCountUpdate += ThreadContext_OnFinishTask;
                threadContext.Initialize();
                _threads.TryAdd(i, threadContext);
                _threadsAvailable.Enqueue(threadContext);
            }
        }

        private void ThreadContext_OnFinishTask(ThreadContext context, int amountOfTasksOnQueue)
        {
            if(amountOfTasksOnQueue == 0)
            {
                _threadsAvailable.Enqueue(context);
                _waitThreadAvailable.Set();
            }
        }

        public void Run(Action action)
        {
            if (_threadsAvailable.TryDequeue(out ThreadContext? availableThread))
            {
                availableThread.EqueueAction(action);
            }
            else
            {
                _processQueue.Enqueue(new Action[] { action });
                _waitProcessQueue.Set();
            }   
        }

        public void RunSynchronously(params Action[] actions)
        {
            if (_threadsAvailable.TryDequeue(out ThreadContext? availableThread))
            {
                foreach(Action action in actions)
                {
                    availableThread.EqueueAction(action);
                }
            }
            else
            {
                _processQueue.Enqueue(actions);
                _waitProcessQueue.Set();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            if (_isDisposed)
            {
                return;
            }
            _cancellationToken.Cancel();
            foreach (var threadContext in _threads)
            {
                threadContext.Value.Stop();
            }
            _isDisposed = true;
        }
    }
}