using BenchmarkDotNet.Running;

namespace BalanceServiceBenchmark;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<BalanceBenchmark>();
    }
}
