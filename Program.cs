using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            //BenchmarkRunner.Run<SqlServerBenchmark>();

            //BenchmarkRunner.Run<IgniteSqlBenchmark>();

            //BenchmarkRunner.Run<CasterBenchmark>(ManualConfig
            //    .Create(DefaultConfig.Instance)
            //    .With(Job.RyuJitX64.WithLaunchCount(1).WithWarmupCount(1)));

            //BenchmarkRunner.Run<IgniteSerializationBenchmark>();

            BenchmarkRunner.Run<IgniteLinqBenchmark>();
        }
    }
}
