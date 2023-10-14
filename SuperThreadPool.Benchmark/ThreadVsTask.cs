using BenchmarkDotNet.Attributes;
using SuperThreadPool.Benchmarks.Config;
using System.Text;
using System.Threading;

namespace SuperThreadPool.Benchmarks
{
    [Config(typeof(McAfeeCheatConfig))]
    //[SimpleJob(launchCount: 3, warmupCount: 10, invocationCount: 30)]
    public class ThreadVsTask
    {
        public static int threadPoolSize = 200;
        public static int tasks = 200;
        private static ThreadPool _myThreadPool;
        public ThreadVsTask()
        {
            if(_myThreadPool == null)
            {
                _myThreadPool = new SuperThreadPool.ThreadPool(threadPoolSize);
            }
        }

        [Benchmark]
        public void RunUsingThread()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            int expected = tasks;
            int current = 0;
            for (int i = 0; i < tasks; i++)
            {
                // this is one thread per process
                Thread thread = new Thread(() =>
                {
                    TooMuchWork();
                    Interlocked.Increment(ref current);
                    if (current == expected)
                    {
                        autoResetEvent.Set();
                    }
                });
                thread.Start();
            }
            autoResetEvent.WaitOne();
        }

        [Benchmark]
        public void RunUsingTask()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            int expected = tasks;
            int current = 0;
            bool done = false;
            for (int i = 0; i < tasks; i++)
            {
                Task.Run(() =>
                {
                    TooMuchWork();
                    Interlocked.Increment(ref current);
                    if (current == expected)
                    { 
                        autoResetEvent.Set();
                    }
                });
            }
            autoResetEvent.WaitOne();
        }

        [Benchmark]
        public void RunUsingSuperThreadPool()
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            int expected = tasks;
            int current = 0;
            var threadPool = _myThreadPool;
            for (int i = 0; i < tasks; i++)
            {
                threadPool.Run(() =>
                {
                    TooMuchWork();
                    Interlocked.Increment(ref current);
                    if (current == expected)
                    {
                        autoResetEvent.Set();
                    }
                });
            }
            autoResetEvent.WaitOne();
        }

        public string TooMuchWork()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10_000; i++)
            {
                sb.Append(i.ToString());
            }
            return sb.ToString();
        }
    }
}
