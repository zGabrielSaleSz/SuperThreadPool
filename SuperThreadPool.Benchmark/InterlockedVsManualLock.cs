namespace SuperThreadPool.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using SuperThreadPool.Benchmarks.Config;
    using System.Threading.Tasks;

    [Config(typeof(McAfeeCheatConfig))]
    public class InterlockedVsManualLock
    {
        [Benchmark]
        public void InterlockedCost()
        {
            int valu = 0;
            for(long i = 0; i < 1_000_000; i++)
            {
                Task.Run(() =>
                {
                    Interlocked.Increment(ref valu);
                });
            }
            
        }


        [Benchmark]
        public void ManualCost()
        {
            object lockRef = new object();
            int valu = 0;
            for (long i = 0; i < 1_000_000; i++)
            {
                Task.Run(() =>
                {
                    lock(lockRef)
                    {
                        valu++;
                    }
                });
            }

        }
    }
}
