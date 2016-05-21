using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            //new IgniteSqlBenchmark();
            //BenchmarkRunner.Run<SqlServerBenchmark>();
            //BenchmarkRunner.Run<IgniteSqlBenchmark>();
            BenchmarkRunner.Run<CasterBenchmark>(ManualConfig
                .Create(DefaultConfig.Instance)
                .With(Job.RyuJitX64.WithLaunchCount(1).WithWarmupCount(1)));
        }
    }
}
