using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<SqlBenchmark>();
        }
    }
}
