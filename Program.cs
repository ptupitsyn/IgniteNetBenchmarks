using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<PlatformCacheBenchmark>();
            //BenchmarkRunner.Run<IgniteSerializationBenchmark>();
            //BenchmarkRunner.Run<CasterBenchmark>();
            //BenchmarkRunner.Run<IgniteLinqBenchmark>();
        }
    }
}
