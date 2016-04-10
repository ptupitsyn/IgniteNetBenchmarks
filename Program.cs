using BenchmarkDotNet.Running;

namespace IgniteNetBenchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<SqlServerBenchmark>();
            //BenchmarkRunner.Run<IgniteSqlBenchmark>();
        }
    }
}
