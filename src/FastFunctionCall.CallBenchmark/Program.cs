using BenchmarkDotNet.Running;

namespace FastFunctionCall.CallBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<FunctionCallBenchmark>();
        }
    }
}
