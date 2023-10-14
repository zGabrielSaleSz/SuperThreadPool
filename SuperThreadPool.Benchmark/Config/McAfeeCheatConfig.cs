namespace SuperThreadPool.Benchmarks.Config
{
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
    public class McAfeeCheatConfig : ManualConfig
    {
        public McAfeeCheatConfig()
        {
            AddJob(Job.MediumRun
                .WithToolchain(InProcessNoEmitToolchain.Instance));
        }
    }
}
