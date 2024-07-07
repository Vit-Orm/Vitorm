using BenchmarkDotNet.Attributes;

namespace App.OrmRunner
{

    //[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    [InProcess]
    public class BenchmarkRunner
    {
        [Params(100)]
        public int N;

        [Params(10, 100, 500, 1000)]
        public int rowCount;

        [Params(true, false)]
        public bool queryJoin;

        [Params(typeof(Runner_EntityFramework), typeof(Runner_Vitorm))]
        public Type testType;

        IRunner queryTest;

        [GlobalSetup]
        public void Setup()
        {
            queryTest = Activator.CreateInstance(testType) as IRunner;
        }

        [Benchmark]
        public void Run()
        {
            var config = new RunConfig { repeatCount = N, queryJoin = queryJoin, take = rowCount };
            queryTest.Run(config);
        }
    }


}