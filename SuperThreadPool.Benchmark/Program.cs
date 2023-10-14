using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text;

namespace SuperThreadPool.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Benchmark SuperThreadPool");

            //GetMaxThreadCount();
            var result = BenchmarkRunner.Run<ThreadVsTask>();
        }


        private static void GetMaxThreadCount()
        {
            int count = 0;
            var threadList = new List<Thread>();
            try
            {
                while (true)
                {
                    Thread newThread = new Thread(new ThreadStart(DummyCall), 1024);
                    newThread.Start();
                    threadList.Add(newThread);
                    count++;
                    Console.WriteLine($"Thread count {count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The limit of threads are {count}");
            }
        }

        private static void DummyCall()
        {
            Thread.Sleep(TimeSpan.MaxValue);
        }
    }
}