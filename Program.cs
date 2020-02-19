using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<ConcurrentDictionaryBenchmark>();
            //BenchmarkRunner.Run<IgniteSerializationBenchmark>();
            //BenchmarkRunner.Run<CasterBenchmark>();
            //BenchmarkRunner.Run<IgniteLinqBenchmark>();
        }
    }
}
