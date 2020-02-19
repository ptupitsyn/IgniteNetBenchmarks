using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            //BenchmarkRunner.Run<IgniteSerializationBenchmark>();
            BenchmarkRunner.Run<CasterBenchmark>();
            //BenchmarkRunner.Run<IgniteLinqBenchmark>();
        }
    }
}
